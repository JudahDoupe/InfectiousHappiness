using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undyed : Block
{

	void Start ()
    {
	    Class = "Block";
        Type = "Undyed";
	    UpdateMaterial();
    }

    public override void Collide(Droplet droplet)
    {
        if (droplet.Type != "")
            Voxel.Fill(EntityConstructor.NewBlock(droplet.Type), Voxel.Puzzle.Number);
        Destroy(droplet.gameObject);
        VoxelWorld.ActiveLevel.ActivePuzzle.UpdateActiveBlocks(VoxelWorld.MainCharacter.Load,true);
    }

}
