using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevLevel : LevelBuilder {

    void Awake()
    {
        Setup();

        BuildStartingRoom();
        BuildMobilityRoom();
        BuildMovementBlockRoom();

    }

    void BuildStartingRoom()
    {
        CurrentRoom = Rooms[0];
        PlaceFloor(new Vector3(1, 0, 1), new Vector3(3, 0, 3));
        CurrentRoom.FillVoxel(new Vector3(2, 0, 0), FloorBlock);
        CurrentRoom.FillVoxel(new Vector3(2, 0, 4), FloorBlock);
        CurrentRoom.FillVoxel(new Vector3(0, 0, 2), FloorBlock);
        CurrentRoom.FillVoxel(new Vector3(4, 0, 2), FloorBlock);
        
    }

    void BuildMobilityRoom()
    {
        CurrentRoom = Rooms[1];
        PlaceFloor(new Vector3(0, 0, 0), new Vector3(2, 0, 10));
        PlacePath(new Vector3(1, 0, 2), new Vector3(1, 1, 2));
        PlacePath(new Vector3(1, 0, 3), new Vector3(1, 2, 3));
        PlacePath(new Vector3(1, 0, 5), new Vector3(1, 2, 5));
        PlacePath(new Vector3(1, 0, 6), new Vector3(1, 3, 6));
        CurrentRoom.FillVoxel(new Vector3(1, 0, 8), BounceBlock);
        PlacePath(new Vector3(1, 0, 10), new Vector3(1, 3, 10));
        CurrentRoom.FillVoxel(new Vector3(1, 3, 10), GoalBlock);
    }

    void BuildMovementBlockRoom()
    {
        CurrentRoom = Rooms[2];
    }
}
