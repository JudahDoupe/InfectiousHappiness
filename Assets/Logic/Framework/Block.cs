﻿using System.Collections;
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

        private Movement _movement;
        private Voxel _topVoxel;
        private bool goalMet;

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
            if (Type != BlockType.Movable) return;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Push);
            _movement.StartCoroutine("MoveToVoxel", World.GetVoxel(transform.position + pusher.transform.forward));
        }
        public void Punch(Character puncher)
        {
            if (Type != BlockType.Movable) return;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            _movement.StartCoroutine("MoveToVoxel", World.GetVoxel(transform.position + puncher.transform.forward * 2));
        }
        public bool Lift(Character lifter)
        {
            if (Type != BlockType.Movable) return false;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            _movement.Parent(lifter.Movement);
            _movement.JumpToVoxel(World.GetVoxel(lifter.transform.position + lifter.transform.up));
            return true;
        }
        public bool Drop(Character dropper)
        {
            if (Type != BlockType.Movable) return false;

            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);

            _movement.UnParent();
            if (_movement.JumpToVoxel(World.GetVoxel(dropper.transform.position + dropper.transform.forward)))
                return true;

            _movement.Parent(dropper.Movement);
            _movement.MoveToVoxel(World.GetVoxel(transform.position));
            return false;
        }
        public bool Activate()
        {
            if (IsActivated) return false;

            IsActivated = true;
            Score.Value++;

            gameObject.GetComponentInChildren<MeshRenderer>().material = ActivatedMaterial;

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
