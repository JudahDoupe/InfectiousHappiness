using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeTemplate : LevelTemplate
{

    public override void Build(Level level)
    {
        Setup(level);

        BuildStartingPlatform();

    }

    void BuildStartingPlatform()
    {
        CurrentRoomTemplate = 0;
        PlaceFloor(new Vector3(-5, 0, -5), new Vector3(5, 0, 5));
    }
}

