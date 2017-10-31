using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public GameObject FloorBlock;
    public GameObject MovableBlock;
    public GameObject GoalBlock;
    public GameObject BounceBlock;

    public Vector3 LevelPosition;
    public Vector3 StartPosition;

    private Level _level;

    public void Build()
    {
        _level = new Level(LevelPosition);
        _level.StartingVoxel = _level.GetVoxel(StartPosition);

        Map.Levels.Add(_level);
        Map.CurrentLevel = _level;
    }

    public Vector3 PlaceBlock(Vector3 pos, GameObject block)
    {
        _level.PlaceNewBlock(pos, block);
        return pos;
    }
    public Vector3 PlacePath(Vector3 start, Vector3 end)
    {
        var direction = (end - start).normalized;
        var distance = Vector3.Distance(start, end);
        for (var t = 0f; t <= distance; t += 0.25f)
        {
            PlaceBlock(start + (direction * t), FloorBlock);
        }

        return end;
    }
    public Vector3 PlaceFloor(Vector3 start, Vector3 end)
    {
        for (var x = start.x; x <= end.x; x++)
        {
            for (var y = start.y; y <= end.y; y++)
            {
                for (var z = start.z; z <= end.z; z++)
                {
                    PlaceBlock(new Vector3(x, y, z), FloorBlock);
                }
            }
        }
        return end;
    }
}
