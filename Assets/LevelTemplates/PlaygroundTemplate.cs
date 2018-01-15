﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundTemplate : LevelTemplate
{
    public override void Build(Level level)
    {
        Setup(level);

        BuildStartingRoom();
        BuildMobilityRoom();
        BuildMovementBlockRoom();
        BuildPipeBlockRoom();
        BuildSwitchBlockRoom();

        Level.IsLoaded = true;
    }

    void BuildStartingRoom()
    {
        CurrentRoomTemplate = 0;
        PlaceFloor(new Vector3(1, 0, 1), new Vector3(3, 0, 3));
        PlaceBlock(new Vector3(2, 0, 0), BlockType.Floor);
        PlaceBlock(new Vector3(2, 0, 4), BlockType.Floor);
        PlaceBlock(new Vector3(0, 0, 2), BlockType.Floor);
        PlaceBlock(new Vector3(4, 0, 2), BlockType.Floor);
    }

    void BuildMobilityRoom()
    {
        CurrentRoomTemplate = 1;

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(2, 0, 10));
        PlaceHallway(new Vector3(1, 0, 2), new Vector3(1, 1, 2));
        PlaceHallway(new Vector3(1, 0, 3), new Vector3(1, 2, 3));
        PlaceHallway(new Vector3(1, 0, 5), new Vector3(1, 2, 5));
        PlaceHallway(new Vector3(1, 0, 6), new Vector3(1, 3, 6));
        PlaceBlock(new Vector3(1, 0, 8), BlockType.Bounce);
        PlaceHallway(new Vector3(1, 0, 10), new Vector3(1, 3, 10));
        PlaceBlock(new Vector3(1, 3, 10), BlockType.Goal);
    }

    void BuildMovementBlockRoom()
    {
        CurrentRoomTemplate = 2;

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(10, 0, 4));
        PlaceBlock(new Vector3(8, 1, 2), BlockType.Movable);
        PlaceBlock(new Vector3(8, 2, 2), BlockType.Floor);
        PlaceBlock(new Vector3(4, 1, 2), BlockType.Floor);
        PlaceBlock(new Vector3(3, 0, 2), BlockType.Bounce);
        PlaceBlock(new Vector3(2, 1, 2), BlockType.Floor);
        PlaceBlock(new Vector3(2, 2, 2), BlockType.Goal);
    }

    void BuildPipeBlockRoom()
    {
        CurrentRoomTemplate = 3;

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(6, 0, 6));
        PlacePipe(new Vector3(1, 1, 2), new Vector3(1, 1, 5));
        PlacePipe(new Vector3(1, 1, 5), new Vector3(1, 3, 5));
        PlacePipe(new Vector3(1, 3, 5), new Vector3(3, 3, 5));
        PlacePipe(new Vector3(3, 3, 5), new Vector3(3, 7, 5));
        PlacePipe(new Vector3(3, 7, 5), new Vector3(3, 7, 2));
        PlacePipe(new Vector3(3, 7, 1), new Vector3(3, 6, 1));

        PlaceFloor(new Vector3(2, 1, 0), new Vector3(4, 3, 2));
        PlaceBlock(new Vector3(3, 3, 1), BlockType.Goal);

        PlacePipe(new Vector3(5, 1, 5), new Vector3(5, 1, 1));
    }

    void BuildSwitchBlockRoom()
    {
        CurrentRoomTemplate = 4;

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(6, 0, 6));
        PlaceFloor(new Vector3(0, 0, 5), new Vector3(6, 2, 5));
        PlaceBlock(new Vector3(3, 1, 5), BlockType.Switch);

        PlaceBlock(new Vector3(3, 0, 1), BlockType.Switch);

        PlaceFloor(new Vector3(2, -1, 3), new Vector3(4, -1, 5));
        PlaceBlock(new Vector3(3, -1, 4), BlockType.Goal);
    }
}