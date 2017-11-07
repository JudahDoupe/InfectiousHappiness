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
        public Room Room;

        private Voxel _topVoxel;
        private Material _originalMaterial;

        void Start()
        {
            var movement = gameObject.GetComponent<Movement>();
            if (Type == BlockType.Movable && movement == null)
                gameObject.AddComponent<Movement>();

            var mesh = gameObject.GetComponentInChildren<MeshRenderer>();
            if (mesh)
                _originalMaterial = mesh.material;
        }
        void Update()
        {
            if (_topVoxel == null)
                _topVoxel = World.GetVoxel(transform.position - World.GravityVector.normalized);
            if (Type == BlockType.Switch)
            {
                if (_topVoxel == null)
                    IsActivated = false;
            }
            if (Type == BlockType.Pipe)
            {
                var neighbors = World.GetNeighboringVoxels(transform.position)
                    .Where(v => v.HasBlock() && v.GetBlock().Type == BlockType.Pipe).ToList();

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

                    if(gameObject.GetComponentInChildren<MeshRenderer>().material != ActivatedMaterial)
                        gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;

                }
                else
                {
                    transform.localScale = new Vector3(1,1,1);
                    if(_originalMaterial != null && gameObject.GetComponentInChildren<MeshRenderer>().material != _originalMaterial)
                        gameObject.GetComponentInChildren<MeshRenderer>().material = _originalMaterial;
                }
            }
        }

        public bool Activate(Character activator = null)
        {
            if (IsActivated) return false;

            switch (Type)
            {
                case BlockType.Floor:
                    IsActivated = true;
                    Score.Value++;
                    gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;
                    return true;
                case BlockType.Goal:
                    IsActivated = true;
                    Score.Value += 10;
                    gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;
                    Room.CompleteRoom();
                    return true;
                case BlockType.Switch:
                    if (activator == null) return false;
                    IsActivated = true;
                    var path = new Stack<Voxel>();
                    path.Push(World.GetVoxel(activator.transform.position));
                    activator.Movement.Transport(GetPipePath(BlockType.Switch, path));
                    return true;
                case BlockType.Pipe:
                    if (World.GetNeighboringVoxels(transform.position)
                            .Count(v => v.HasBlock() && v.GetBlock().Type == BlockType.Pipe) > 1) return false;
                    if (activator == null) return false;
                    activator.Movement.Transport(GetPipePath(BlockType.Pipe));
                    return true;
                default:
                    return false;
            }
        }
        public bool Stand(Movement stander = null)
        {
            switch (Type)
            {
                case BlockType.Floor:
                    Activate();
                    return true;
                case BlockType.Goal:
                    Activate();
                    return true;
                case BlockType.Bouncy:
                    if(stander)
                        stander.Bounce();
                    return false;
                case BlockType.Switch:
                    if (stander == null) return false;
                    World.GravityVector  = new Vector3(World.GravityVector.x, -World.GravityVector.y, World.GravityVector.z);
                    stander.transform.Rotate(stander.transform.forward,180);
                    Activate(stander.GetComponent<Character>());
                    return true;
                default:
                    return true;
            }
        }

        public Stack<Voxel> GetPipePath(BlockType type ,Stack<Voxel> currentPath = null)
        {
            if (currentPath == null)
                currentPath = new Stack<Voxel>();
            currentPath.Push(World.GetVoxel(transform.position));

            var nextPath = World.GetNeighboringVoxels(transform.position)
                .FirstOrDefault(v => v.HasBlock() && v.GetBlock().Type == type && !currentPath.Contains(v));

            if (nextPath == null)
            {
                currentPath.Pop();
                var end = World.GetVoxel(transform.position + (transform.position - currentPath.Pop().Position));
                currentPath.Push(World.GetVoxel(transform.position));
                currentPath.Push(end);
                return currentPath;
            }
            return nextPath.GetBlock().GetPipePath(type, currentPath);
        }
    }

    public enum BlockType
    {
        Static,
        Floor,
        Goal,
        Movable,
        Bouncy,
        Pipe,
        Switch,
    }
}
