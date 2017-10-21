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

    public void Forward()
    {
        if (Stunned) return;

        var vox = Map.GetVoxel(Voxel.Position + transform.forward);
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
}
