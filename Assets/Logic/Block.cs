using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Block : MonoBehaviour
{
    private BlockType _type;
    public BlockType Type
    {
        get { return _type; }
        set
        {
            _type = value;

            _materials = MaterialManager.GetBlockMaterials(_type);
            Renderer.material = IsActivated ? _materials.ActiveMaterial : _materials.InactiveMaterial;

            if (Type == BlockType.Movable || Type == BlockType.Bounce || Type == BlockType.Pipe || Type == BlockType.Falling)
            {
                if (_movement == null)
                    _movement = gameObject.AddComponent<Movement>();
            }
            else
            {
                if (_movement != null)
                {
                    Destroy(_movement);
                    _movement = null;
                }
            }
        }
    }
    public void SetType(string typeName)
    {
        switch (typeName)
        {
            case "Static":
                Type = BlockType.Static;
                break;
            case "Floor":
                Type = BlockType.Floor;
                break;
            case "Goal":
                Type = BlockType.Goal;
                break;
            case "Movable":
                Type = BlockType.Movable;
                break;
            case "Bounce":
                Type = BlockType.Bounce;
                break;
            case "Pipe":
                Type = BlockType.Pipe;
                break;
            case "Switch":
                Type = BlockType.Switch;
                break;
            case "Falling":
                Type = BlockType.Falling;
                break;
            default:
                Type = BlockType.Static;
                break;
        }
    }
    public bool IsActivated;

    private Movement _movement;
    private Renderer _renderer;
    public Renderer Renderer
    {
        get { return _renderer ?? (_renderer = gameObject.GetComponentInChildren<Renderer>()); }
    }
    private BlockMaterialSet _materials;

    void Start()
    {
        _movement = gameObject.GetComponent<Movement>();
        Type = _type;
    }
    void Update()
    {
        if (Type == BlockType.Pipe)
            UpdatePipe();
        if (Type == BlockType.Falling && IsActivated)
        {
            if (!_movement.IsFalling && Vector3.Distance(VoxelWorld.Instance.MainCharacter.transform.position, transform.position) > 1)
            {
                _movement.Fall();
            }
        }
    }

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
    public bool Stand(Movement stander = null)
    {
        switch (Type)
        {
            case BlockType.Bounce:
                StandBounce(stander);
                return false;
            case BlockType.Switch:
                StandSwitch(stander);
                return true;
            default:
                Activate();
                return true;
        }
    }

    public void Activate()
    {
        if (IsActivated) return;

        IsActivated = true;
        Renderer.material = _materials.ActiveMaterial;

        if (_type == BlockType.Goal)
        {
            var voxel = VoxelWorld.GetVoxel(transform.position);
            if (voxel.Room != null) voxel.Room.Complete();
        }
    }
    public void Deactivate()
    {
        if (!IsActivated) return;

        IsActivated = false;
        Renderer.material = _materials.InactiveMaterial;
    }

    private void StandBounce(Movement stander)
    {
        if (stander != null) stander.Bounce();
    }
    private void StandSwitch(Movement stander)
    {
        if (IsActivated || stander == null || stander.gameObject.GetComponent<Character>() == null)
        {
            IsActivated = false;
            return;
        }

        VoxelWorld.GravityVector = Vector3.Scale(VoxelWorld.GravityVector, new Vector3(1,-1,1));
        stander.transform.Rotate(stander.transform.forward, 180);

        var path = new Stack<Voxel>();
        path.Push(VoxelWorld.GetVoxel(stander.transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position + (transform.position - stander.transform.position)));

        if (stander.Transport(path)) IsActivated = true;
    }

    private bool InteractSwitch(Movement stander)
    {
        if (IsActivated || stander == null) return true;

        stander.transform.Rotate(stander.transform.up, 180);

        var path = new Stack<Voxel>();
        path.Push(VoxelWorld.GetVoxel(stander.transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position));
        path.Push(VoxelWorld.GetVoxel(transform.position + (transform.position - stander.transform.position)));

        return stander.Transport(path);
    }
    private bool InteractPipe(Movement stander)
    {
        if (IsActivated || stander == null) return true;

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
            var end = VoxelWorld.GetVoxel(transform.position + (transform.position - currentPath.Pop().Position));
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
            var direction = (neighbors[0].Position - neighbors[1].Position).normalized;

            if (Mathf.Abs(direction.x) > 0.999)
                transform.localScale = new Vector3(1.5f, 0.75f, 0.75f);
            else if (Mathf.Abs(direction.y) > 0.999)
                transform.localScale = new Vector3(0.75f, 1.5f, 0.75f);
            else if (Mathf.Abs(direction.z) > 0.999)
                transform.localScale = new Vector3(0.75f, 0.75f, 1.5f);
            else
                transform.localScale = new Vector3(1, 1, 1);

            if (gameObject.GetComponentInChildren<MeshRenderer>().material != _materials.ActiveMaterial)
                gameObject.GetComponentInChildren<MeshRenderer>().material = _materials.ActiveMaterial;

        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (_materials.InactiveMaterial != null && gameObject.GetComponentInChildren<MeshRenderer>().material != _materials.InactiveMaterial)
                gameObject.GetComponentInChildren<MeshRenderer>().material = _materials.InactiveMaterial;
        }
    }
}

public enum BlockType
{
    Static,
    Floor,
    Goal,
    Movable,
    Bounce,
    Pipe,
    Switch,
    Falling,
}
