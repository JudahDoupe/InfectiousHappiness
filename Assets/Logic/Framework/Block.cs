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

        void Start()
        {
            var movement = gameObject.GetComponent<Movement>();
            if (Type == BlockType.Movable && movement == null)
                gameObject.AddComponent<Movement>();
        }
        void Update()
        {
            if (Type == BlockType.Goal)
            {
                if(_topVoxel == null)
                    _topVoxel = World.GetVoxel(transform.position - World.GravityVector.normalized);
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
                    return true;
                case BlockType.Pipe:
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
                    Activate();
                    return true;
                default:
                    return true;
            }
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
