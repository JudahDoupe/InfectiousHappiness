using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IOManager : MonoBehaviour {

    private static bool debug = true;
    private static string levelsDirPath;

    public void Awake()
    {
        levelsDirPath = Application.persistentDataPath + "/LevelData";
        if (!Directory.Exists(levelsDirPath))
            Directory.CreateDirectory(levelsDirPath);
    }

    public static void SaveRoom(RoomData2 roomData)
    {
        var json = JsonUtility.ToJson(roomData);
        File.WriteAllText(GetFilePath(roomData.LevelName, roomData.RoomNum), json);

        if (debug) Debug.Log("Saving Room " + roomData.RoomNum + " in Level" + roomData.LevelName);
    }
    public static RoomData2 LoadRoom(string levelName, int roomNum)
    {
        var filePath = GetFilePath(levelName, roomNum);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new RoomData2();
        }

        string jsonData = File.ReadAllText(filePath);
        var roomData = JsonUtility.FromJson<RoomData2>(jsonData);
        return roomData;
    }

    public static void SaveLevel(LevelData2 data, string name)
    {
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetFilePath(name), json);

        if(debug) Debug.Log("Saving Level Info");
    }
    public static LevelData2 LoadLevel(string name)
    {
        var filePath = GetFilePath(name);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new LevelData2();
        }

        string jsonData = File.ReadAllText(filePath);
        var levelData = JsonUtility.FromJson<LevelData2>(jsonData);
        return levelData;
    }
    public static LevelData LoadLevelOld(string name)
    {
        var filePath = levelsDirPath + "/" + name + ".json";

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: "+filePath);
            return new LevelData();
        }

        string jsonData = File.ReadAllText(filePath);
        var levelData = JsonUtility.FromJson<LevelData>(jsonData);
        return levelData;
    } // Depreciated

    public static AudioClip LoadTrack(string name)
    {
        return Resources.Load<AudioClip>(name);
    }
    public static GameObject LoadObject(string objectName)
    {
        return Resources.Load<GameObject>(objectName);
    }

    private static string GetFilePath(string levelName, int roomNum = -1)
    {
        levelName = levelName.ToLower().Replace(" ", "_");
        var levelDir = levelsDirPath + "/" + levelName;

        if (!Directory.Exists(levelDir))
            Directory.CreateDirectory(levelDir);

        var filePath = levelDir + "/" + levelName;
        if (roomNum >= 0)
            filePath += "_room" + roomNum;
        filePath += ".json";

        return filePath;
    }
}

[Serializable]
public struct LevelData
{
    public string Name;
    public Vector3 Pos;
    public Vector3 SpawnPos;

    public RoomData[] Rooms;
    public VoxelData[] Voxels;
}
[Serializable]
public struct RoomData
{
    public string Name;
    public int RoomNum;
    public string TrackName;
}
[Serializable]
public struct VoxelData
{
    public string ObjectName;
    public Vector3 WorldPosition;
    public int RoomNum;
    public bool IsActive;
}


[Serializable]
public struct LevelData2
{
    public string Name;
    public Vector3 Pos;
    public Vector3 SpawnPos;

    public bool PlayerCanJump;
    public bool PlayerCanPush;
    public bool PlayerCanLift;
    public bool PlayerCanPipe;
    public bool PlayerCanSwitch;
}
[Serializable]
public struct RoomData2
{
    public string LevelName;
    public string Name;
    public int RoomNum;
    public string TrackName;
    public Vector3 RoomOffset;
    public VoxelData2[] Voxels;
}
[Serializable]
public struct VoxelData2
{
    public string Object; 
    public string ObjectType; 
    public Vector3 LvlPos;
    public bool Active;
}