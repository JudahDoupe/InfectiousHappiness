using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Block : MonoBehaviour
    {
        public BlockType Type = BlockType.Static;
        public int InfectionLevel;
        public bool IsInfected;
        public Material InfectedMaterial;

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
                    _topVoxel = Map.GetVoxel(transform.position - Map.GravityDirection);
                if (_topVoxel.HasBlock() && _topVoxel.GetBlock().Type == BlockType.Movable)
                    Map.InfectBlocksBelowLevel(InfectionLevel);
            }
        }

        public void Push(Character pusher)
        {
            if (Type != BlockType.Movable) return;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Push);
            _movement.StartCoroutine("MoveToVoxel", Map.GetVoxel(transform.position + pusher.transform.forward));
        }
        public void Punch(Character puncher)
        {
            if (Type != BlockType.Movable) return;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            _movement.StartCoroutine("MoveToVoxel", Map.GetVoxel(transform.position + puncher.transform.forward * 2));
        }
        public bool Lift(Character lifter)
        {
            if (_movement.IsBeingCarried || Type != BlockType.Movable) return false;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            _movement.IsBeingCarried = true;
            _movement.JumpToVoxel(Map.GetVoxel(lifter.transform.position + lifter.transform.up));
            return true;
        }
        public bool Drop(Character dropper)
        {
            if (!_movement.IsBeingCarried || Type != BlockType.Movable) return false;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);

            _movement.IsBeingCarried = false;
            _movement.IsStunned = false;
            if (_movement.JumpToVoxel(Map.GetVoxel(dropper.transform.position + dropper.transform.forward)))
                return true;

            _movement.IsBeingCarried = true;
            _movement.IsStunned = true;
            return false;
        }
        public bool Infect()
        {
            if (IsInfected) return false;

            IsInfected = true;
            Map.Score++;
            Map.InfectBlocksBelowLevel(InfectionLevel);

            gameObject.GetComponentInChildren<MeshRenderer>().material = InfectedMaterial;

            return true;
        }
    }

    public enum BlockType
    {
        Static,
        Movable,
        Bouncy,
        Goal
    }
}
