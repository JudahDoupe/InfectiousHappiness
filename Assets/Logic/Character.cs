using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;


public class Character : MonoBehaviour
{
    public Voxel Voxel;
    public Vector3 Direction = Vector3.forward;
    public float Speed = 0.25f;
    public bool Stunned;

    public IEnumerator MoveToVoxel(Voxel vox)
    {
        Stunned = true;
        Voxel = vox;

        var block = Map.GetVoxel(vox.Position + Vector3.down).Block;
        if (block)
            block.Infect();

        while (Vector3.Distance(transform.position, vox.Position) > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, vox.Position, Speed);
            yield return new WaitForFixedUpdate();
        }

        transform.position = vox.Position;
        Stunned = false;
    }

    public IEnumerator ArchToVoxel(Voxel vox, int height)
    {
        Stunned = true;
        Voxel = vox;

        var block = Map.GetVoxel(vox.Position + Vector3.down).Block;
        if (block)
            block.Infect();
        var frames = Speed * 60;
        for (var t = 0; t < frames; t++)
        {
            var vOffset = (frames / 2) - t;
            transform.position = Vector3.Lerp(transform.position, vox.Position + transform.up * vOffset, t / frames);

            yield return new WaitForFixedUpdate();
            
        }

        transform.position = vox.Position;
        Stunned = false;
    }

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
        Direction = Quaternion.Euler(0, 90, 0) * Direction;
    }
    public void Left()
    {
        if (Stunned) return;
        transform.Rotate(new Vector3(0, -90, 0));
        Direction = Quaternion.Euler(0, -90, 0) * Direction;
    }

    public void Jump()
    {
        if (Stunned) return;
    }
    public void Bounce()
    {
        if (Stunned) return;
    }
    public void Leap()
    {
        if (Stunned) return;
    }

    public void Push()
    {
        if (Stunned) return;
    }
    public void Pull()
    {
        if (Stunned) return;
    }
    public void Punch()
    {
        if (Stunned) return;
    }

    public void Climb()
    {
        if (Stunned) return;

        var vox = Map.GetVoxel(Voxel.Position + transform.forward + Vector3.up);
        StartCoroutine(ArchToVoxel(vox, 1));
    }
    public void Vault()
    {
        if (Stunned) return;
    }
    public void Switch()
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
