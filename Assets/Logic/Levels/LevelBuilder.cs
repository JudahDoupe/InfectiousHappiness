using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEditor;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [Serializable]
    public struct LevelBuilderRoomData
    {
        public Vector3 LevelPos;
        public AudioClip Track;
    }

    public Vector3 SpawnPosition = new Vector3(Level.Size/2,0, Level.Size / 2);
    public Vector3 WorldPosition;
    public string FilePath = "/LevelData/new_level.json";
    public List<LevelBuilderRoomData> RoomInfo = new List<LevelBuilderRoomData>();

    public Level Level;
    protected int CurrentRoom;

    public virtual Level Build()
    {
        return new Level(FilePath, Vector3.zero);
    }

    public void Setup()
    {
        Level = new Level(FilePath, WorldPosition, SpawnPosition);
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