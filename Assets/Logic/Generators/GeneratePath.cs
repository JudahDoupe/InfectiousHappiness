using System.Collections;
using System.Collections.Generic;
using Assets.Logic.Blocks;
using UnityEngine;

public class GeneratePath : MonoBehaviour
{
    public GameObject BlockPrefab;

	void Start () {
        var pos = Vector3.zero;
	    Block prev = null;

	    for (var i=0;i<10;i++)
	    {
	        if (prev == null)
	        {
	            var blockObject = Instantiate(BlockPrefab, pos, Quaternion.identity);
	            var block = blockObject.GetComponent<Block>();

	            prev = block;
            }
            else
	        {
	            pos = prev.transform.position + prev.transform.forward;

                var blockObject = Instantiate(BlockPrefab, pos ,Quaternion.identity);
	            blockObject.transform.parent = gameObject.transform;
	            var block = blockObject.GetComponent<Block>();

                block.Neighbors.Add(Direction.South,prev);
                prev.Neighbors.Add(Direction.North, block);

	            prev = block;
	        }
	    }
	}

}
