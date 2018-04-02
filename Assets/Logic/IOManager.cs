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
    public static IOManager Instance;

    public void Awake()
    {
        Instance = this;

        DirectoryInfo dataDir = new DirectoryInfo(Application.persistentDataPath);
        dataDir.Delete(true);

        for (var i = 0; i < LevelsToTransfer.Length; i++)
        {
            var levelName = LevelsToTransfer[i];
            CopyLevelIfNonexistent(levelName);
        }
    }

    public static void SavePuzzle(PuzzleData puzzleData)
    {
        if (Instance.DebugOutput) Instance.DebugOutput.text = "Saving Puzzle " + puzzleData.PuzzleNum + " in Level" + puzzleData.LevelName;

        var json = JsonUtility.ToJson(puzzleData);
        File.WriteAllText(GetLevelFilePath(puzzleData.LevelName, puzzleData.PuzzleNum), json);

        if (Instance.DebugOutput) Instance.DebugOutput.text = "Saved Puzzle " + puzzleData.PuzzleNum + " in Level" + puzzleData.LevelName;
    }
    public static PuzzleData LoadPuzzle(string levelName, int puzzleNum)
    {
        var filePath = GetLevelFilePath(levelName, puzzleNum);

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
        if (Instance.DebugOutput) Instance.DebugOutput.text = "Saving Level" + data.Name;

        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetLevelFilePath(data.Name), json);

        if (Instance.DebugOutput) Instance.DebugOutput.text = "Saved Level" + data.Name;
    }
    public static LevelData LoadLevel(string name)
    {
        var filePath = GetLevelFilePath(name);

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
    public static GameObject LoadObject(string objectName)
    {
        return Resources.Load<GameObject>(objectName);
    }

    private static string GetLevelFilePath(string levelName, int puzzleNum = -1)
    {
        var levelsDirPath = Application.persistentDataPath + "/LevelData";
        if (!Directory.Exists(levelsDirPath))
            Directory.CreateDirectory(levelsDirPath);

        levelName = levelName.ToLower().Replace(" ", "_");
        var levelDir = levelsDirPath + "/" + levelName;

        if (!Directory.Exists(levelDir))
            Directory.CreateDirectory(levelDir);

        var filePath = levelDir + "/" + levelName;
        if (puzzleNum >= 0)
            filePath += "_room" + puzzleNum;
        filePath += ".json";

        return filePath;
    }
    private static string GetResourceLevelPath(string levelName, int puzzleNum = -1)
    {
        levelName = levelName.ToLower().Replace(" ", "_");
        var levelDir = "Levels/" + levelName;

        var filePath = levelDir + "/" + levelName;
        if (puzzleNum >= 0)
            filePath += "_room" + puzzleNum;

        return filePath;
    }
    public void CopyLevelIfNonexistent(string levelName)
    {

        var filePath = GetLevelFilePath(levelName);
        if (!File.Exists(filePath))
        {
            var json = Resources.Load<TextAsset>(GetResourceLevelPath(levelName));
            File.WriteAllText(filePath, json.text);
            for (int i = 0; i < Level.NumPuzzles; i++)
            {
                var roomFilePath = GetLevelFilePath(levelName, i);
                if (!File.Exists(roomFilePath))
                {
                    json = Resources.Load<TextAsset>(GetResourceLevelPath(levelName, i));
                    File.WriteAllText(roomFilePath, json.text);
                }
            }
        }

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

