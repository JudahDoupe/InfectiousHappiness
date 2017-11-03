using System.Collections;
using System.Collections.Generic;
using Assets.Logic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public GameObject FloorBlock;
    public GameObject MovableBlock;
    public GameObject GoalBlock;
    public GameObject BounceBlock;

    public Vector3 SpawnPosition;
    public Vector3 WorldPosition;
    public List<Vector3> RoomPositions = new List<Vector3>();
    public List<AudioSource> RoomTracks = new List<AudioSource>();

    private Level _level;
    public List<Room> Rooms = new List<Room>();
    public Room CurrentRoom;

    public void Setup()
    {
        _level = World.AddLevel(WorldPosition);
        _level.SpawnVoxel = _level.GetVoxel(SpawnPosition);

        foreach (var roomPosition in RoomPositions)
        {
            Rooms.Add(_level.AddRoom(roomPosition));
        }
    }

    public Vector3 PlacePath(Vector3 start, Vector3 end)
    {
        if (CurrentRoom == null) return start;

        var direction = (end - start).normalized;
        var distance = Vector3.Distance(start, end);
        for (var t = 0f; t <= distance; t += 0.25f)
        {
            CurrentRoom.FillVoxel(start + (direction * t), FloorBlock);
        }

        return end;
    }
    public Vector3 PlaceFloor(Vector3 start, Vector3 end)
    {
        if (CurrentRoom == null) return start;

        for (var x = start.x; x <= end.x; x++)
        {
            for (var y = start.y; y <= end.y; y++)
            {
                for (var z = start.z; z <= end.z; z++)
                {
                    CurrentRoom.FillVoxel(new Vector3(x, y, z), FloorBlock);
                }
            }
        }
        return end;
    }
}