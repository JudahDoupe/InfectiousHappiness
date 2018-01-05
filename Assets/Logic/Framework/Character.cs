using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEngine;


public class Character : MonoBehaviour
{

    public Movement Load;
    public Movement Movement;

    private int _roomNum = 0;

    void Start()
    {

        Movement = gameObject.GetComponent<Movement>() ?? gameObject.AddComponent<Movement>();
        Movement.SpawnVoxel = VoxelWorld.SpawnVoxel;
        Movement.Fall();
    }

    void Update()
    {
        Movement.enabled = true;
        if (Input.GetButtonDown("Up"))
            Forward();
        else if (Input.GetButtonDown("Down"))
            Back();
        else if (Input.GetButtonDown("Left"))
            Left();
        else if (Input.GetButtonDown("Right"))
            Right();
        else if (Input.GetButtonDown("Primary"))
            Primary();
        else if (Input.GetButtonDown("Secondary"))
            Secondary();
        else if (Input.GetKeyDown(KeyCode.L))
            VoxelWorld.LoadLevel(VoxelWorld.ActiveLevel);
        else if (Input.GetKeyDown(KeyCode.U))
            VoxelWorld.UnLoadLevel(VoxelWorld.ActiveLevel);
        else if (Input.GetKeyDown(KeyCode.Escape))
            Time.timeScale = Time.timeScale <= 0.001 ? 1 : 0;
    }

    // Input Commands
    public void Forward()
    {
        if (GetForwardBlock() != null)
            Climb();
        else if (GetForwardGap() == null && GetForwardGapFloor() == null)
            Jump();
        else
            Movement.MoveToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward));
    }
    public void Back()
    {
        Movement.MoveToVoxel(VoxelWorld.GetVoxel(transform.position - transform.forward));
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
                f.GetComponent<Movement>().Push(this);
                break;
            case BlockType.Switch:
                f.Interact(this);
                break;
            case BlockType.Pipe:
                f.Interact(this);
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
                f.GetComponent<Movement>().Lift(this);
                break;
            case BlockType.Switch:
                f.Interact(this);
                break;
            default:
                break;
        }
    }

    // Movement
    public void Jump()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward * 2));
    }
    public void Climb()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward - VoxelWorld.GravityVector.normalized));
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
        var vox = VoxelWorld.GetVoxel(transform.position - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardBlock()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardGap()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardGapFloor()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up * 2);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }

}
