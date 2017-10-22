using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic.World;
using UnityEngine;


public class Character : Movable
{

    public static Character Instance;
    public Block _carrying;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    //Movement Commands

    public void Forward()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position + transform.forward);
        if(target.Block == null)
            StartCoroutine(MoveToVoxel(target));
    }
    public void Back()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position - transform.forward);
        if (target.Block == null)
            StartCoroutine(MoveToVoxel(target));
    }
    public void Right()
    {
        if (Stunned) return;
        transform.Rotate(new Vector3(0, 90, 0));
    }
    public void Left()
    {
        if (Stunned) return;
        transform.Rotate(new Vector3(0, -90, 0));
    }

    //Special Commands

    public void Jump()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position - transform.up + transform.forward * 2);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward * 2),
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward * 2),
        };

        if (air.All(v => v.Block == null))
        {
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Jump);
            StartCoroutine(JumpOnVoxel(target));
        }
    }
    public void Leap()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position - transform.up + transform.forward * 3);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward * 2),
            Map.GetVoxel(Voxel.Position + transform.forward * 3),
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward * 2),
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward * 3)
        };

        if (air.All(v => v.Block == null))
        {
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Jump);   
            StartCoroutine(JumpOnVoxel(target));
        }
    }

    public void Climb()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position + transform.forward);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position + transform.up + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.up),
        };

        if (target.Block != null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(target));
    }
    public void Vault()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position + transform.forward + transform.up);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position + transform.up),
            Map.GetVoxel(Voxel.Position + transform.up * 2),
            Map.GetVoxel(Voxel.Position + transform.up * 2 + transform.forward),
        };

        if (target.Block != null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(target));
    }
    public void Switch()
    {
        if (Stunned) return;

        transform.Rotate(new Vector3(0, 0, 180));
        //Camera.main.transform.Rotate(new Vector3(0, 0, 180));
        StartCoroutine(Fall());

        Debug.Log(transform.rotation);
    }

    //Block Commands

    public void Push()
    {
        if (Stunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (!block) return;

        var mover = block.gameObject.GetComponent<Movable>();
        if(mover)
            mover.RecievePush();
    }
    public void Punch()
    {
        if (Stunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (!block) return;

        var mover = block.gameObject.GetComponent<Movable>();
        if (mover)
            mover.RecievePunch();
    }

    public void Lift()
    {
        if (Stunned) return;

        var block = Map.GetVoxel(transform.position + transform.forward).Block;

        if (!block) return;

        SoundFX.Instance.PlayClip(SoundFX.Instance.LiftVoice);
        var moveable = block.gameObject.GetComponent<Movable>();
        if (moveable && moveable.RecieveLift())
            _carrying = block;
    }
    public void Drop()
    {
        if (Stunned || _carrying == null) return;

        var moveable = _carrying.gameObject.GetComponent<Movable>();
        SoundFX.Instance.PlayClip(SoundFX.Instance.DropVoice);
        if (moveable && moveable.RecieveDrop())
            _carrying = null;
    }
}
