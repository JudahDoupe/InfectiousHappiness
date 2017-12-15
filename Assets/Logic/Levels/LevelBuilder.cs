using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [Serializable]
    public struct LevelBuilderRoomData
    {
        public Vector3 LevelPos;
        public AudioClip Track;
    }

    public Vector3 SpawnPosition;
    public Vector3 WorldPosition;
    public List<LevelBuilderRoomData> RoomInfo = new List<LevelBuilderRoomData>();

    public Level Level;
    public int CurrentRoom;

    public void Setup()
    {
        Level = VoxelWorld.AddLevel(new Level(WorldPosition, SpawnPosition));
        var i = 0;
        foreach (var data in RoomInfo)
        {
            Level.GetRoom(i).BuildRoom(data.Track);
            i++;
        }
    }

    public Vector3 PlaceHallway(Vector3 start, Vector3 end)
    {
        var direction = (end - start).normalized;
        var distance = Vector3.Distance(start, end);
        for (var t = 0f; t <= distance; t += 0.25f)
        {
            PlaceBlock(start + (direction * t), VoxelWorld.Instance.FloorBlock);
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
                    PlaceBlock(new Vector3(x, y, z),VoxelWorld.Instance.FloorBlock);
                }
            }
        }
        return end;
    }
    public Vector3 PlacePipe(Vector3 start, Vector3 end)
    {
        start += RoomInfo[CurrentRoom].LevelPos;
        end += RoomInfo[CurrentRoom].LevelPos;

        var direction = (end - start).normalized;
        var distance = Vector3.Distance(start, end);
        for (var t = 0f; t <= distance; t += 0.25f)
        {
            PlaceBlock(start + (direction * t), VoxelWorld.Instance.PipeBlock);
        }

        return end;
    }
    public Vector3 PlaceBlock(Vector3 pos, GameObject prefab)
    {
        pos += RoomInfo[CurrentRoom].LevelPos;
        var vox = Level.GetVoxel(pos);
        if (vox == null) return pos;
        var obj = Instantiate(prefab, vox.Position, Quaternion.identity);
        vox.Fill(obj, CurrentRoom);
        return vox.Position;
    }
}