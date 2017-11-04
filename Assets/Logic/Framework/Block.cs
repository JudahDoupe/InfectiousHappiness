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

        private Movement _movement;
        private Voxel _topVoxel;

        void Start()
        {
            _movement = gameObject.GetComponent<Movement>();
            if (Type == BlockType.Movable && _movement == null)
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

        public void Push(Character pusher)
        {
            if (Type != BlockType.Movable || _movement.IsStunned) return;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Push);
            _movement.StartCoroutine("MoveToVoxel", World.GetVoxel(transform.position + pusher.transform.forward));
        }
        public bool Lift(Character lifter)
        {
            if (Type != BlockType.Movable || _movement.IsStunned) return false;

            lifter.Load = this;
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            _movement.Parent(lifter.Movement);
            _movement.JumpToVoxel(World.GetVoxel(lifter.transform.position + lifter.transform.up));
            return true;
        }
        public bool Drop(Character dropper)
        {
            if (Type != BlockType.Movable || _movement.IsStunned) return false;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);

            dropper.Load = null;
            _movement.UnParent();
            if (_movement.JumpToVoxel(World.GetVoxel(dropper.transform.position + dropper.transform.forward)))
                return true;

            _movement.Parent(dropper.Movement);
            _movement.MoveToVoxel(World.GetVoxel(transform.position));
            return false;
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
