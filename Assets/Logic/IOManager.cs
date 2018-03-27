using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IOManager : MonoBehaviour
{
    public Text DebugOutput;
    public string[] LevelsToTransfer;

    private static bool debug = true;
    private static string _levelsDirPath;
    private static string _defaultLevelsDirPath;

    public void Awake()
    {
        DebugOutput.text = "Starting IO Manager";

        _levelsDirPath = Application.persistentDataPath + "/LevelData";
        _defaultLevelsDirPath = Application.dataPath + "/Levels";

        DebugOutput.text = "Creating Perminant Directtory";
        if (!Directory.Exists(_levelsDirPath))
            Directory.CreateDirectory(_levelsDirPath);
        DebugOutput.text = "Permenant Directory Created";

        for (var i = 0; i < LevelsToTransfer.Length; i++)
        {
            var levelName = LevelsToTransfer[i];
            DebugOutput.text = "Saving Level " + levelName;
            SaveLevel(LoadDefaultLevel(levelName));
            DebugOutput.text = "Saved Level "+ levelName;
            for (var j = 0; j < Level.NumPuzzles; j++)
            {
                SavePuzzle(LoadDefaultPuzzle(levelName, j));
                DebugOutput.text = "Saved " + levelName + " Puzzle " + j;
            }
        }

        DebugOutput.text = "All Saved";
    }

    public static void SavePuzzle(PuzzleData puzzleData)
    {
        var json = JsonUtility.ToJson(puzzleData);
        File.WriteAllText(GetFilePath(puzzleData.LevelName, puzzleData.PuzzleNum), json);

        if (debug) Debug.Log("Saving Puzzle " + puzzleData.PuzzleNum + " in Level" + puzzleData.LevelName);
    }
    public static PuzzleData LoadPuzzle(string levelName, int puzzleNum)
    {
        var filePath = GetFilePath(levelName, puzzleNum);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new PuzzleData();
        }

        string jsonData = File.ReadAllText(filePath);
        var roomData = JsonUtility.FromJson<PuzzleData>(jsonData);
        return roomData;
    }
    public static PuzzleData LoadDefaultPuzzle(string levelName, int puzzleNum)
    {
        var filePath = GetDefaultFilePath(levelName, puzzleNum);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new PuzzleData();
        }

        string jsonData = File.ReadAllText(filePath);
        var roomData = JsonUtility.FromJson<PuzzleData>(jsonData);
        return roomData;
    }

    public static void SaveLevel(LevelData data)
    {
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetFilePath(data.Name), json);

        if(debug) Debug.Log("Saving Level Info");
    }
    public static LevelData LoadLevel(string name)
    {
        var filePath = GetFilePath(name);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new LevelData();
        }

        string jsonData = File.ReadAllText(filePath);
        var levelData = JsonUtility.FromJson<LevelData>(jsonData);
        return levelData;
    }
    public static LevelData LoadDefaultLevel(string name)
    {
        var filePath = GetDefaultFilePath(name);

        if (!File.Exists(filePath))
        {
            Debug.Log("No file at: " + filePath);
            return new LevelData();
        }

        string jsonData = File.ReadAllText(filePath);
        var levelData = JsonUtility.FromJson<LevelData>(jsonData);
        return levelData;
    }

    public static AudioClip LoadTrack(string name)
    {
        return Resources.Load<AudioClip>(name);
    }

    private static string GetFilePath(string levelName, int puzzleNum = -1)
    {
        levelName = levelName.ToLower().Replace(" ", "_");
        var levelDir = _levelsDirPath + "/" + levelName;

        if (!Directory.Exists(levelDir))
            Directory.CreateDirectory(levelDir);

        var filePath = levelDir + "/" + levelName;
        if (puzzleNum >= 0)
            filePath += "_room" + puzzleNum;
        filePath += ".json";

        return filePath;
    }
    private static string GetDefaultFilePath(string levelName, int puzzleNum = -1)
    {
        levelName = levelName.ToLower().Replace(" ", "_");
        var levelDir = _defaultLevelsDirPath + "/" + levelName;

        if (!Directory.Exists(levelDir))
            Directory.CreateDirectory(levelDir);

        var filePath = levelDir + "/" + levelName;
        if (puzzleNum >= 0)
            filePath += "_room" + puzzleNum;
        filePath += ".json";

        return filePath;
    }
}

[Serializable]
public struct LevelData
{
    public string Name;
    public Vector3 Position;
    public Vector3 SpawnPosition;
    public Vector3 SpawnRotation;
}
[Serializable]
public struct PuzzleData
{
    public string LevelName;
    public string PuzzleName;
    public int PuzzleNum;
    public string TrackName;
    public Vector3 PuzzleOffset;

    public VoxelData[] Voxels;
}
[Serializable]
public struct VoxelData
{
    public string Object; 
    public string ObjectType; 
    public Vector3 LvlPos;
    public bool Active;
}