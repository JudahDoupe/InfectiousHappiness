using System.Collections;
using System.Collections.Generic;
using Assets.Logic.Blocks;
using UnityEngine;


public class Character : MonoBehaviour
{
    public Block StandingBlock;
    public float speed;

    public Direction Facing = Direction.North;

    public void Forward()
    {
        Block block;
        Vector3 pos;
        if (StandingBlock.Neighbors.TryGetValue(Facing, out block))
        {
            pos = block.Top;
        }
        else
        {
            pos = transform.position + transform.forward;
        }
        StartCoroutine("forward", pos);
    }
    private IEnumerable forward(Vector3 pos)
    {
        yield return new WaitForFixedUpdate();

    }
}
