using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;


public class Character : MonoBehaviour
{
    public Block StandingBlock;
    public float Speed = 0.25f;
    public bool Stunned;
    public Vector3 Direction = Vector3.forward;

    public void Forward()
    {
        Block block = StandingBlock.GetNeighbor(Direction);
        if (!Stunned && block != null)
        {
            var pos = block.Top;
            StandingBlock = block;
            StartCoroutine(MoveForward(pos));
        }
    }
    private IEnumerator MoveForward(Vector3 pos)
    {
        Stunned = true;
        while ( Vector3.Distance(transform.position,pos) > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position,pos,Speed);
            yield return new WaitForFixedUpdate();
        }
        transform.position = pos;
        Stunned = false;
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
