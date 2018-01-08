using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTemplate : LevelBuilder
{

    public override Level Build()
    {
        Setup();

        BuildStartingRoom();

        return Level;
    }

    void BuildStartingRoom()
    {
        CurrentRoom = 0;

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(47, 0, 47));
        PlaceBlock(new Vector3(2, 0, 0), VoxelWorld.Instance.FloorBlock);
    }

}