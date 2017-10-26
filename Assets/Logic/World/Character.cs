using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic.World;
using UnityEngine;


public class Character : MonoBehaviour
{

    public Block Load;

    private Movement _movement;

    void Start()
    {
        _movement = gameObject.GetComponent<Movement>();
        if (_movement == null)
            gameObject.AddComponent<Movement>();
    }

    //Movement Commands

    public void MoveForward()
    {
        _movement.MoveToVoxel(Map.GetVoxel(transform.position + transform.forward));
    }
    public void MoveBack()
    {
        _movement.MoveToVoxel(Map.GetVoxel(transform.position - transform.forward));
    }
    public void MoveRight()
    {
        _movement.MoveToVoxel(Map.GetVoxel(transform.position + transform.right));
    }
    public void MoveLeft()
    {
        _movement.MoveToVoxel(Map.GetVoxel(transform.position - transform.right));
    }

    public void TurnRight()
    {
        if (_movement.IsStunned) return;
        transform.Rotate(new Vector3(0, 90, 0));
    }
    public void TurnLeft()
    {
        if (_movement.IsStunned) return;
        transform.Rotate(new Vector3(0, -90, 0));
    }

    //Special Commands

    public void Jump()
    {
        _movement.JumpToVoxel(Map.GetVoxel(transform.position + transform.forward * 2));
    }
    public void Leap()
    {
        _movement.JumpToVoxel(Map.GetVoxel(transform.position + transform.forward * 3));
    }

    public void Climb()
    {
        _movement.JumpToVoxel(Map.GetVoxel(transform.position + transform.forward - Map.GravityDirection));
    }
    public void Vault()
    {
        _movement.JumpToVoxel(Map.GetVoxel(transform.position + transform.forward - Map.GravityDirection * 2));
    }
    public void Switch()
    {
        Map.GravityDirection = -Map.GravityDirection;
        transform.Rotate(new Vector3(0, 0, 180));
        _movement.Fall();
    }

    public void Die()
    {
        Drop();
        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
    }

    //Block Commands

    public void Push()
    {
        if (_movement.IsStunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (block) block.Push(this);
    }
    public void Punch()
    {
        if (_movement.IsStunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (block) block.Punch(this);
    }

    public void Lift()
    {
        if (_movement.IsStunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (block) block.Lift(this);
    }
    public void Drop()
    {
        if (_movement.IsStunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (block) block.Drop(this);
    }
}
