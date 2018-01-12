using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IOController : MonoBehaviour {

    private static bool debug = true;
    private static string levelDirPath;

    public void Awake()
    {
        levelDirPath = Application.persistentDataPath + "/LevelData";
        if (!Directory.Exists(levelDirPath))
            Directory.CreateDirectory(levelDirPath);
    }

    public static void SaveLevel(LevelData data, string name)
    {
        var filePath = levelDirPath + "/" + name + ".json";
        var json = JsonUtility.ToJson(data);

        File.WriteAllText(filePath, json);

        if(debug) Debug.Log("Saving Level");
    }
    public static LevelData LoadLevel(string name)
    {
        var filePath = levelDirPath + "/" + name + ".json";

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: "+filePath);
            return new LevelData();
        }

        string jsonData = File.ReadAllText(filePath);
        return JsonUtility.FromJson<LevelData>(jsonData);
    }
    public static AudioClip LoadTrack(string name)
    {
        return Resources.Load<AudioClip>(name);
        
    }
    public static GameObject LoadObject(string name)
    {
        return Resources.Load<GameObject>(name);
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
