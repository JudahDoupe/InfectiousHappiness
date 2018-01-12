using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelTemplate : MonoBehaviour
{
    [Serializable]
    public struct RoomTemplateData
    {
        public string Name;
        public Vector3 PositionInLevel;
        public string AudioTrackName;
    }

    public Vector3 SpawnPosition = new Vector3(Level.Size/2,0, Level.Size / 2);
    public Vector3 PositionInWorld;
    public List<RoomTemplateData> RoomTemplates = new List<RoomTemplateData>();

    public Level Level;
    protected int CurrentRoomTemplate;

    public virtual void Build(Level level){}

    public void Setup(Level level)
    {
        Level = level;
        Level.SpawnVoxel = Level.GetVoxel(SpawnPosition);
        Level.WorldPostition = PositionInWorld;
        var i = 0;
        foreach (var data in RoomTemplates)
        {
            Level.GetRoom(i).Load(new RoomData {Name = data.Name,RoomNum = i,TrackName = data.AudioTrackName});
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
        pos += RoomTemplates[CurrentRoomTemplate].PositionInLevel;
        var vox = Level.GetVoxel(pos);
        if (vox == null) return pos;
        var obj = Instantiate(prefab, vox.Position, Quaternion.identity);
        vox.Fill(obj, CurrentRoomTemplate);
        return vox.Position;
    }
}
