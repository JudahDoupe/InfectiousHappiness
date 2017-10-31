using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevLevel : LevelBuilder {

    void Awake()
    {
        Build();

        PlaceFloor(new Vector3(0, 0, 0), new Vector3(49, 0, 49));

        // Block Tests
        PlaceBlock(new Vector3(25, 1, 7), MovableBlock);

        // Goal Test
        PlacePath(new Vector3(20, 1, 10), new Vector3(20, 5, 15));
        PlaceBlock(new Vector3(20, 0, 16), BounceBlock);
        var pillar = PlacePath(new Vector3(20, 1, 17), new Vector3(20, 5, 17));
        PlaceBlock(pillar, GoalBlock);
        PlaceBlock(new Vector3(20, 1, 7), MovableBlock);

        // Movement Test
        PlacePath(new Vector3(30, 1, 7), new Vector3(30, 2, 8));
        PlacePath(new Vector3(30, 1, 10), new Vector3(30, 2, 10));
        PlacePath(new Vector3(30, 1, 11), new Vector3(30, 4, 11));
        PlacePath(new Vector3(30, 1, 13), new Vector3(30, 4, 13));
        PlacePath(new Vector3(30, 1, 16), new Vector3(30, 4, 16));
    }
}
