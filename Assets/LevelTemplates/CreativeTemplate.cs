using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeTemplate : LevelTemplate
{

    public override void Build(Level level)
    {
        Setup(level);

        BuildStartingPlatform();

        Level.IsLoaded = true;
    }

    void BuildStartingPlatform()
    {
        CurrentRoomTemplate = 0;
        PlaceFloor(new Vector3(-5, -1, -5) + SpawnPosition, new Vector3(5, -1, 5) + SpawnPosition);
    }
}

