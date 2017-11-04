using System.Linq;
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

    void Update()
    {
        if (Input.GetButtonDown("Up"))
            Forward();
        else if (Input.GetButtonDown("Down"))
            Back();
        else if (Input.GetButtonDown("Left"))
            Left();
        else if (Input.GetButtonDown("Right"))
            Right();
        else if (Input.GetButtonDown("Jump"))
            Jump();
        else if (Input.GetButtonDown("Primary"))
            Primary();
        else if (Input.GetButtonDown("Secondary"))
            Secondary();
    }

    // Input Commands
    public void Forward()
    {
        if (GetForwardBlock() != null)
            Climb();
        else if (GetForwardGap() == null && GetForwardGapFloor() == null)
            Jump();
        else
            Movement.MoveToVoxel(World.GetVoxel(transform.position + transform.forward));
    }
    public void Back()
    {
        Movement.MoveToVoxel(World.GetVoxel(transform.position - transform.forward));
    }
    public void Right()
    {
        transform.Rotate(new Vector3(0, 90, 0));
    }
    public void Left()
    {
        transform.Rotate(new Vector3(0, -90, 0));
    }
    public void Primary()
    {
        if (Movement.IsStunned) return;
        var f = GetForwardBlock();
        if (f == null) return;

        switch (f.Type)
        {
            case BlockType.Movable:
                f.Push(this);
                break;
            case BlockType.Switch:
                f.Activate(this);
                break;
            case BlockType.Pipe:
                f.Activate(this);
                break;
            default:
                break;
        }
    }
    public void Secondary()
    {
        if (Movement.IsStunned) return;
        if (Load != null)
        {
            Load.Drop(this);
            return;
        }

        var f = GetForwardBlock();
        if (f == null) return;

        switch (f.Type)
        {
            case BlockType.Movable:
                f.Lift(this);
                break;
            case BlockType.Switch:
                f.Activate(this);
                break;
            default:
                break;
        }
    }

    // Movement
    public void Jump()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward * 2));
    }
    public void Climb()
    {
        Movement.JumpToVoxel(World.GetVoxel(transform.position + transform.forward - World.GravityVector.normalized));
    }
    public void Die()
    {
        if (Load != null)
            Load.Drop(this);
        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
    }

    // Queries
    public Block GetFloorBlock()
    {
        var vox = World.GetVoxel(transform.position - transform.up);
        if (vox != null && vox.HasBlock())
            return vox.GetBlock();

        return null;
    }
    public Block GetForwardBlock()
    {
        var vox = World.GetVoxel(transform.position + transform.forward);
        if (vox != null && vox.HasBlock())
            return vox.GetBlock();

        return null;
    }
    public Block GetForwardGap()
    {
        var vox = World.GetVoxel(transform.position + transform.forward - transform.up);
        if (vox != null && vox.HasBlock())
            return vox.GetBlock();

        return null;
    }
    public Block GetForwardGapFloor()
    {
        var vox = World.GetVoxel(transform.position + transform.forward - transform.up * 2);
        if (vox != null && vox.HasBlock())
            return vox.GetBlock();

        return null;
    }

}
