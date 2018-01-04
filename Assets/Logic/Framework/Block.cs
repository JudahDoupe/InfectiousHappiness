using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Logic.UI;
using System.Linq;
using UnityEngine;

namespace Assets.Logic.Framework
{
    public class Block : MonoBehaviour
    {
        public BlockType Type = BlockType.Static;
        public Material ActivatedMaterial;
        public bool IsActivated;

        private Material _originalMaterial;

        void Start()
        {
            var movement = gameObject.GetComponent<Movement>();
            if ((Type == BlockType.Movable || Type == BlockType.Bounce || Type == BlockType.Pipe) && movement == null)
                gameObject.AddComponent<Movement>();

            var mesh = gameObject.GetComponentInChildren<MeshRenderer>();
            if (mesh)
                _originalMaterial = mesh.material;
        }
        void Update()
        {
            if (Type == BlockType.Pipe)
                UpdatePipe();
        }

        public bool Interact(Character activator = null)
        {
            switch (Type)
            {
                case BlockType.Switch:
                    return SwitchInteract(activator == null ? null : activator.Movement);
                case BlockType.Pipe:
                    return PipeInteract(activator == null ? null : activator.Movement);
                default:
                    return false;
            }
        }
        public bool Stand(Movement stander = null)
        {
            switch (Type)
            {
                case BlockType.Floor:
                    FloorActivate(stander);
                    return true;
                case BlockType.Goal:
                    GoalActivate(stander);
                    return true;
                case BlockType.Bounce:
                    BounceStand(stander);
                    return false;
                case BlockType.Switch:
                    SwitchStand(stander);
                    return true;
                default:
                    return true;
            }
        }

        public void FloorActivate(Movement stander)
        {
            if (IsActivated || (stander != null && stander.gameObject.GetComponent<Character>() == null)) return;

            IsActivated = true;
            Score.Value++;
            gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;
        }
        public void GoalActivate(Movement stander)
        {
            if (IsActivated) return;

            IsActivated = true;
            Score.Value += 10;
            gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;
            VoxelWorld.GetVoxel(transform.position).Room.Complete();
        }
        public void BounceStand(Movement stander)
        {
            if (stander != null) stander.Bounce();
        }
        public void SwitchStand(Movement stander)
        {
            if (IsActivated || stander == null)
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
        public bool SwitchInteract(Movement stander)
        {
            if (IsActivated || stander == null) return true;

            stander.transform.Rotate(stander.transform.up, 180);

            var path = new Stack<Voxel>();
            path.Push(VoxelWorld.GetVoxel(stander.transform.position));
            path.Push(VoxelWorld.GetVoxel(transform.position));
            path.Push(VoxelWorld.GetVoxel(transform.position + (transform.position - stander.transform.position)));

            return stander.Transport(path);
        }
        public bool PipeInteract(Movement stander)
        {
            if (IsActivated || stander == null) return true;

            // if we are not activating an end o a pipe
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

                if (gameObject.GetComponentInChildren<MeshRenderer>().material != ActivatedMaterial)
                    gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;

            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
                if (_originalMaterial != null && gameObject.GetComponentInChildren<MeshRenderer>().material != _originalMaterial)
                    gameObject.GetComponentInChildren<MeshRenderer>().material = _originalMaterial;
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
    }
}
