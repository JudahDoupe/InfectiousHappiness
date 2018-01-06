using System.Collections;
using System.Collections.Generic;
using Assets.Logic.Framework;
using UnityEngine;

public class CreativeLevel : LevelBuilder
{

    public override Level Build()
    {
        Setup();

        BuildStartingPlatform();

        return Level;
    }

    void BuildStartingPlatform()
    {
        CurrentRoom = 0;
        PlaceBlock(SpawnPosition + Vector3.down, VoxelWorld.Instance.FloorBlock);
    }
}
