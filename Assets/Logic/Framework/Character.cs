﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEngine;


public class Character : MonoBehaviour
{

    public Block Load;
    public Movement Movement;

    void Start()
    {
        Movement = gameObject.GetComponent<Movement>();
        if (Movement == null)
            gameObject.AddComponent<Movement>();
        Movement.SpawnVoxel = World.SpawnVoxel;
        Movement.Fall();
    }

    //Movement Commands

    public void MoveForward()
    {
        Movement.MoveToVoxel(World.GetVoxel(transform.position + transform.forward));
    }
    public void MoveBack()
    {
        Movement.MoveToVoxel(World.GetVoxel(transform.position - transform.forward));
    }
    public void MoveRight()
    {
        Movement.MoveToVoxel(World.GetVoxel(transform.position + transform.right));
    }
    public void MoveLeft()
    {
        Movement.MoveToVoxel(World.GetVoxel(transform.position - transform.right));
    }

    public void TurnRight()
    {
        if (Movement.IsStunned) return;
        transform.Rotate(new Vector3(0, 90, 0));
    }
    public void TurnLeft()
    {
        if (Movement.IsStunned) return;
        transform.Rotate(new Vector3(0, -90, 0));
    }

    //Special Commands

    public void Jump()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward * 2));
    }
    public void Leap()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward * 3));
    }

    public void Climb()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward - World.GravityVector.normalized));
    }
    public void Vault()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward - World.GravityVector.normalized * 2));
    }
    public void Switch()
    {
        World.GravityVector = -World.GravityVector;
        transform.Rotate(new Vector3(0, 0, 180));
        Movement.Fall();
    }

    public void Die()
    {
        Drop();
        //SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
    }

    //Block Commands

    public void Push()
    {
        if (Movement.IsStunned) return;

        var vox = World.GetVoxel(transform.position + transform.forward);

        if (vox.HasBlock()) vox.GetBlock().Push(this);
    }
    public void Punch()
    {
        if (Movement.IsStunned) return;

        var vox = World.GetVoxel(transform.position + transform.forward);

        if (vox.HasBlock()) vox.GetBlock().Punch(this);
    }

    public void Lift()
    {
        if (Movement.IsStunned) return;

        var vox = World.GetVoxel(transform.position + transform.forward);

        if (!vox.HasBlock()) return;

        Load = vox.GetBlock();
        if (!Load.Lift(this))
        {
            Load = null;
            return;
        }

        Load.transform.parent = transform;
    }
    public void Drop()
    {
        if (Movement.IsStunned || Load == null) return;

        if (!Load.Drop(this)) return;

        Load.transform.parent = null;
        Load = null;
    }
}
