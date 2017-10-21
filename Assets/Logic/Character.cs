using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic.World;
using UnityEngine;


public class Character : MonoBehaviour
{
    public Voxel Voxel;
    public float Speed = 0.25f;
    public bool Stunned;

    public IEnumerator MoveToVoxel(Voxel vox)
    {
        Stunned = true;
        Voxel = vox;

        var block = Map.GetVoxel(vox.Position + Vector3.down).Block;
        if (block)
        {
            block.Infect();
        }

        while (Vector3.Distance(transform.position, vox.Position) > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, vox.Position, Speed);
            yield return new WaitForFixedUpdate();
        }

        transform.position = vox.Position;

        if (block == null)
            StartCoroutine(Fall());

        Stunned = false;
    }

    public IEnumerator JumpOnVoxel(Voxel vox)
    {
        Stunned = true;
        Voxel = Map.GetVoxel(vox.Position + Vector3.up);
        var height = Vector3.Distance(transform.position, vox.Position);

        var block = Map.GetVoxel(vox.Position).Block;
        if (block)
            block.Infect();
        var frames = Speed * 60;
        for (var t = 0; t < frames; t++)
        {
            var vOffset = (1- t/frames) * height;
            transform.position = Vector3.Lerp(transform.position, Voxel.Position +  transform.up * vOffset, t / frames);

            yield return new WaitForFixedUpdate();
            
        }

        transform.position = Voxel.Position;

        if (block == null)
            StartCoroutine(Fall());

        Stunned = false;

    }

    public IEnumerator Fall()
    {
        Stunned = true;
        var velocity = 0.0f;

        var block = Map.GetVoxel(Voxel.Position - transform.up).Block;

        while (block == null && Map.InsideMap(transform.position))
        {
            velocity += Map.Gravity;
            transform.Translate(transform.up * -velocity);
            Voxel = Map.GetVoxel(transform.position);
            block = Map.GetVoxel(Voxel.Position - transform.up).Block;
            yield return new WaitForFixedUpdate();
        }

        if (block)
        {
            block.Infect();
            var dist = Vector3.Distance(transform.position, Voxel.Position);
            var lastDist = dist;
            while (lastDist <= dist)
            {
                velocity += Map.Gravity;
                transform.Translate(transform.up * -velocity);
                lastDist = dist;
                dist = Vector3.Distance(transform.position, Voxel.Position);
                yield return new WaitForFixedUpdate();
            }
            transform.position = Voxel.Position;
        }
        else //died
        {
            StartCoroutine(MoveToVoxel(Map.StartingVoxel));
        }

        Stunned = false;
    }

    //Movement Commands

    public void Forward()
    {
        if (Stunned) return;

        var vox = Map.GetVoxel(Voxel.Position + transform.forward);
        StartCoroutine(MoveToVoxel(vox));
    }
    public void Back()
    {
        if (Stunned) return;

        var vox = Map.GetVoxel(Voxel.Position - transform.forward);
        StartCoroutine(MoveToVoxel(vox));
    }
    public void Right()
    {
        if (Stunned) return;
        transform.Rotate(new Vector3(0, 90, 0));

        Debug.Log(transform.rotation);
    }
    public void Left()
    {
        if (Stunned) return;
        transform.Rotate(new Vector3(0, -90, 0));

        Debug.Log(transform.rotation);
    }

    //Special Commands

    public void Jump()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position - transform.up + transform.forward * 2);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position - transform.up + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward * 2)
        };

        if (target.Block != null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(target));
    }
    public void Leap()
    {
        if (Stunned) return;

        var target = Map.GetVoxel(Voxel.Position - transform.up + transform.forward * 3);
        var air = new List<Voxel>
        {
            Map.GetVoxel(Voxel.Position - transform.up + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward),
            Map.GetVoxel(Voxel.Position + transform.forward * 2),
            Map.GetVoxel(Voxel.Position + transform.forward * 3)
        };

        if (target.Block != null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(target));
    }
    public void Bounce()
    {
        if (Stunned) return;
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
        StartCoroutine(Fall());
    }

    //Block Commands

    public void Push()
    {
        if (Stunned) return;
    }
    public void Punch()
    {
        if (Stunned) return;
    }

    public void Lift()
    {
        if (Stunned) return;
    }
    public void Drop()
    {
        if (Stunned) return;
    }
}
