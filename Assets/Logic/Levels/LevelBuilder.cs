using System.Collections;
using System.Collections.Generic;
using Assets.Logic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [System.Serializable]
    public struct RoomData
    {
        public Vector3 LevelPos;
        public AudioClip Track;
    }

    public GameObject FloorBlock;
    public GameObject MovableBlock;
    public GameObject GoalBlock;
    public GameObject BounceBlock;

    public Vector3 SpawnPosition;
    public Vector3 WorldPosition;
    public List<RoomData> RoomInfo = new List<RoomData>();

    private Level _level;
    public List<Room> Rooms = new List<Room>();
    public Room CurrentRoom;

    public void Setup()
    {
        _level = World.AddLevel(WorldPosition);
        _level.SpawnVoxel = _level.GetVoxel(SpawnPosition);

        foreach (var data in RoomInfo)
        {
            var room = _level.AddRoom(data.LevelPos);
            Rooms.Add(room);
            if (data.Track != null)
            {
                World.Instance.Music.AddTrack(data.Track, room);
            }
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