using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Block : MonoBehaviour
{
    private BlockType _type;
    private Movement _movement;
    private Renderer _renderer;

    public BlockType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            UpdateMaterial();

            if (IsMoveable() && _movement == null)
                gameObject.AddComponent<Movement>();
            if (!IsMoveable() && _movement != null)
                Destroy(_movement);
        }
    }

    public Movement Movement
    {
        get { return _movement ?? (_movement = gameObject.GetComponentInChildren<Movement>()); }
    }
    public Renderer Renderer
    {
        get { return _renderer ?? (_renderer = gameObject.GetComponentInChildren<Renderer>()); }
    }

    public bool IsDyed;
    public bool IsActive;

    public bool PrimaryInteract(Character activator = null)
    {
        switch (Type)
        {
            case BlockType.Switch:
                return InteractSwitch(activator == null ? null : activator.Movement);
            case BlockType.Pipe:
                return InteractPipe(activator == null ? null : activator.Movement);
            default:
                return false;
        }
    }
    public bool SecondaryInteract(Character activator = null)
    {
        switch (Type)
        {
            default:
                return false;
        }
    }
    public bool Stand(Movement entity = null)
    {
        switch (Type)
        {
            case BlockType.Bounce:
                StandBounce(entity);
                return false;
            case BlockType.Switch:
                StandSwitch(entity);
                return true;
            case BlockType.CheckPoint:
                if (!IsActive)
                    Activate();
                return true;
            default:
                Activate();
                return true;
        }
    }
    public void Exit(Movement entity = null)
    {
        
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdateMaterial();

        if (_type == BlockType.CheckPoint)
        {
            var voxel = VoxelWorld.GetVoxel(transform.position);
            if (voxel.Puzzle != null) voxel.Puzzle.ActivateAllBlocks();
        }
    }
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdateMaterial();
    }

    public bool IsMoveable()
    {
        switch (Type)
        {
            case BlockType.Snow:
                return true;
            default:
                return false;
        }
    }


    private void UpdateMaterial()
    {
        if (!IsDyed)
        {
            Renderer.material = Resources.Load<Material>("Materials/Grey");
        }
        else
        {
            var dyeMaterial = IsActive ? Resources.Load<Material>("Materials/" + _type + "Active") : Resources.Load<Material>("Materials/" + _type);
            Renderer.material = dyeMaterial;
        }
    }

    //Standing Functions
    private void StandBounce(Movement stander)
    {
        if (stander != null) stander.Bounce();
    }
    private void StandSwitch(Movement stander)
    {
        if (IsActive || stander == null || stander.gameObject.GetComponent<Character>() == null)
        {
            IsActive = false;
            return;
        }

        VoxelWorld.GravityVector = Vector3.Scale(VoxelWorld.GravityVector, new Vector3(1,-1,1));
        stander.transform.Rotate(stander.transform.forward, 180);

        var path = new Stack<Voxel>();
        path.Push(VoxelWorld.GetVoxel(stander.transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position + (transform.position - stander.transform.position)));

        if (stander.Transport(path)) IsActive = true;
    }

    //Interation Functions
    private bool InteractSwitch(Movement stander)
    {
        if (IsActive || stander == null) return true;

        stander.transform.Rotate(stander.transform.up, 180);

        var path = new Stack<Voxel>();
        path.Push(VoxelWorld.GetVoxel(stander.transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position + (transform.position - stander.transform.position)));

        return stander.Transport(path);
    }
    private bool InteractPipe(Movement stander)
    {
        if (IsActive || stander == null) return true;

        // if we are not activating an end of a pipe
        if (VoxelWorld.GetNeighboringVoxels(transform.position)
                .Count(v => v.Block && v.Block.Type == BlockType.Pipe) > 1) return false;

        return stander.Transport(GetPipePath(BlockType.Pipe));
    }

    private Stack<Voxel> GetPipePath(BlockType type ,Stack<Voxel> currentPath = null)
    {
        if (currentPath == null)
            currentPath = new Stack<Voxel>();
        currentPath.Push(VoxelWorld.GetVoxel(transform.position));

        var nextPath = VoxelWorld.GetNeighboringVoxels(transform.position)
            .FirstOrDefault(v => v.Block && v.Block.Type == type && !currentPath.Contains(v));

        if (nextPath == null)
        {
            currentPath.Pop();
            var end = VoxelWorld.GetVoxel(transform.position + (transform.position - currentPath.Pop().WorldPosition));
            currentPath.Push(VoxelWorld.GetVoxel(transform.position));
            currentPath.Push(end);
            return currentPath;
        }
        return nextPath.Block.GetPipePath(type, currentPath);
    }
    private void UpdatePipe() 
    {
        var neighbors = VoxelWorld.GetNeighboringVoxels(transform.position)
            .Where(v => v.Block && v.Block.Type == BlockType.Pipe).ToList();

        if (neighbors.Count() > 1)
        {
            var direction = (neighbors[0].WorldPosition - neighbors[1].WorldPosition).normalized;

            if (Mathf.Abs(direction.x) > 0.999)
                transform.localScale = new Vector3(1f, 0.75f, 0.75f);
            else if (Mathf.Abs(direction.y) > 0.999)
                transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            else if (Mathf.Abs(direction.z) > 0.999)
                transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            else
                transform.localScale = new Vector3(1, 1, 1);

            UpdateMaterial();

        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            UpdateMaterial();
        }
    }
}

public enum BlockType
{
    Static,
    CheckPoint,
    Bounce,
    Pipe,
    Switch,
    Stone,
    Snow,
    Ice,
    WoodDark,
    WoodLight,
    LeavesDark,
    LeavesLight,
    Grass,
    Dirt,
    Mud,
    Cloud,
}