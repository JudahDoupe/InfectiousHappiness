using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class VoxelWorld : MonoBehaviour
{
    void Awake()
    {
        if (Instance == null) Instance = this;
        if (MainCharacter == null)
            MainCharacter = FindObjectOfType<Character>();
        
        //Setup Active Level 
        ActiveLevel = new Level(LevelData.Name);
        ActiveLevel.Load();
        ActiveLevel.WorldPostition = LevelData.Position;
        ActiveLevel.SpawnVoxel = ActiveLevel.GetVoxel(LevelData.SpawnPosition);

        //Sync Audio
        AudioSource baseTrack = null;
        for (var i = 0; i < 8; i++)
        {
            if (baseTrack == null) baseTrack = ActiveLevel.GetPuzzle(i).Track;

            var track = ActiveLevel.GetPuzzle(i).Track;
            if (track != null) track.timeSamples = baseTrack.timeSamples;
        }
    }

    // Properties
    public Character MainCharacter;
    public static VoxelWorld Instance;
    public static Voxel SpawnVoxel
    {
        get { return ActiveLevel == null ? null : ActiveLevel.SpawnVoxel; }
    }
    public static Vector3 GravityVector = new Vector3(0, -0.01f, 0);

    [Space(10)]
    //Options
    public bool PlayMusic = true;
    [Space(10)]

    // Level
    public LevelData LevelData;
    public static Level ActiveLevel;

    // Queries
    public static Voxel GetVoxel(Vector3 worldPos)
    {
        if (ActiveLevel != null) return ActiveLevel.GetVoxel(ActiveLevel.WorldToLevel(worldPos));
        Debug.Log("Unable to get voxel because level does not exist at location: "+ worldPos);
        return null;
    }
    public static List<Voxel> GetNeighboringVoxels(Vector3 worldPos)
    {
        return new List<Voxel>
        {
            GetVoxel(worldPos + Vector3.forward),
            GetVoxel(worldPos + Vector3.back),
            GetVoxel(worldPos + Vector3.up),
            GetVoxel(worldPos + Vector3.down),
            GetVoxel(worldPos + Vector3.left),
            GetVoxel(worldPos + Vector3.right),
        }.Where(x => x != null).ToList();
    }
}

public class Level
{
    public const int NumPuzzles = 8;
    public const int Size = 48;
    public string Name;
    public Vector3 WorldPostition;
    public Voxel SpawnVoxel;
    public bool IsLoaded;

    public readonly Puzzle[] Puzzles = new Puzzle[NumPuzzles];
    public readonly Voxel[,,] Voxels = new Voxel[Size, Size, Size];

    public Level(string name)
    {
        Name = name;
    }
    public Level(string name, Vector3 worldPos, Vector3? spawnPos = null)
    {
        Name = name;
        if (spawnPos == null) spawnPos = new Vector3(24, 1, 24);
        WorldPostition = worldPos;
        SpawnVoxel = GetVoxel(spawnPos.Value);
    }
    public void SaveAll()
    {
        Debug.Log("Saving All");
        Save();
        for (int i = 0; i < NumPuzzles; i++)
        {
            GetPuzzle(i).Save();
        }
    }
    public void Save()
    {
        var saveData = new LevelData
        {
            Name = Name,
            Position = WorldPostition,
            SpawnPosition = SpawnVoxel == null ? new Vector3(0, 0, 0) : SpawnVoxel.WorldPosition,
        };
        IOManager.SaveLevel(saveData);
    }
    public void Load()
    {
        if(IsLoaded) return;
        var data = IOManager.LoadLevel(Name);
        if (data.Name == null) return;

        WorldPostition = data.Position;
        SpawnVoxel = GetVoxel(WorldToLevel(data.SpawnPosition));

        for (int i = 0; i < NumPuzzles; i++)
        {
            GetPuzzle(i).Reset();
        }
        IsLoaded = true;
    }
    public void Unload()
    {
        foreach (var puzzle in Puzzles)
        {
            puzzle.Destroy();
        }
        IsLoaded = false;
    }

    public Puzzle GetPuzzle(int roomNum)
    {
        if (roomNum < 0 || roomNum >= NumPuzzles) return null;
        return Puzzles[roomNum] ?? (Puzzles[roomNum] = new Puzzle(this,roomNum));
    }
    public Puzzle GetPuzzle(Vector3 levelPos)
    {
        return GetVoxel(levelPos).Puzzle;
    }
    public Voxel GetVoxel(Vector3 levelPos)
    {
        var pos = new int[3]
        {
            Mathf.RoundToInt(levelPos.x) ,
            Mathf.RoundToInt(levelPos.y) ,
            Mathf.RoundToInt(levelPos.z)
        };

        if (!pos.All(x => x >= 0 && x < Size))
            return null;

        return Voxels[pos[0], pos[1], pos[2]] ??
                (Voxels[pos[0], pos[1], pos[2]] = new Voxel(this, LevelToWorld(new Vector3(pos[0], pos[1], pos[2]))));
    }

    public Vector3 WorldToLevel(Vector3 worldPos)
    {
        return worldPos - WorldPostition;
    }
    public Vector3 LevelToWorld(Vector3 levelPos)
    {
        return levelPos + WorldPostition;
    }
}

public class Puzzle
{
    public Level Level;
    public string Name;
    public int Number;
    public AudioSource Track;
    public Vector3 PuzzleOffset = new Vector3(0,0,0);

    public List<Voxel> Voxels = new List<Voxel>();

    public Puzzle(Level level, int puzzleNum)
    {
        Level = level;
        Number = puzzleNum;
    }
    public void Load(PuzzleData data)
    {
        Destroy();

        Name = data.PuzzleName;
        PuzzleOffset = data.PuzzleOffset;

        if (data.TrackName != "")
        {
            if(Track) Object.Destroy(Track);
            Track = VoxelWorld.Instance.gameObject.AddComponent<AudioSource>();
            Track.clip = IOManager.LoadTrack(data.TrackName);
            Track.volume = 0;
        }
        if (data.Voxels != null)
        {
            for (var i = 0; i < data.Voxels.Length; i++)
            {
                var voxData = data.Voxels[i];
                var vox = Level.GetVoxel(voxData.LvlPos + PuzzleOffset);
                vox.Load(voxData, Number);
            }
        }
    }
    public void Reset()
    {
        var data = IOManager.LoadPuzzle(Level.Name, Number);
        Load(data);
    }
    public void Save()
    {
        IOManager.SavePuzzle(new PuzzleData
        {
            LevelName = Level.Name,
            PuzzleName = Name,
            PuzzleNum = Number,
            PuzzleOffset = PuzzleOffset,
            TrackName = Track == null ? "" : Track.name,
            Voxels = Voxels.Select(voxel => voxel.ToData()).ToArray(),
        });
    }
    public void Destroy()
    {
        foreach (Voxel voxel in Voxels)
        {
            voxel.Destroy();
        }
        Name = "";
        Object.Destroy(Track);
    }

    public void CompletePuzzle()
    {
        if (Level.IsLoaded)
        {
            VoxelWorld.Instance.StartCoroutine(DyeAllBlocks());
        }
    }
    private IEnumerator DyeAllBlocks()
    {
        var i = 0;
        const int speed = 5;
        foreach (var voxel in Voxels)
        {
            if (voxel.Entity is Block)
                (voxel.Entity as Block).Dye();
            i = (i+1) % speed;
            if (i == 0)
                yield return new WaitForFixedUpdate();
        }
        Save();
    }
}

public class Voxel
{
    public Level Level;
    public Puzzle Puzzle;
    public Vector3 WorldPosition;

    public Entity Entity;

    public Voxel(Level level, Vector3 worldPosition)
    {
        Level = level;
        WorldPosition = worldPosition;
    }
    public void Load(VoxelData data, int puzzleNum = -1)
    {
        Fill(EntityConstructor.NewEntity(data.Object, data.ObjectType), puzzleNum);
    }
    public VoxelData ToData()
    {
        var Object = "";
        var ObjectType = "";
        var active = false;

        if (Entity)
        {
            Object = Entity.Class;
            ObjectType = Entity.Type;
        }

        return new VoxelData
        {
            Object = Object,
            ObjectType = ObjectType,
            LvlPos = Level.WorldToLevel(WorldPosition),
            Active = active,
        };
    }
    public void Fill(Entity entity, int puzzleNum = -1)
    {
        Destroy();

        Entity = entity;
        Entity.Voxel = this;
        Entity.transform.position = WorldPosition;
        Entity.transform.parent = VoxelWorld.Instance.transform;

        if(puzzleNum >= 0) ChangePuzzle(puzzleNum);
    }
    public Entity Release()
    {
        var entity = Entity;
        Entity.Voxel = null;
        Entity = null;
        return entity;
    }
    public void Destroy()
    {
        ChangePuzzle(-1);

        if (Entity == null || Entity is Character) return;

        UnityEngine.Object.Destroy(Entity.gameObject);
        Entity = null;
    }
    private void ChangePuzzle(int puzzleNum)
    {
        if (Puzzle != null) Puzzle.Voxels.Remove(this);
        Puzzle = Level.GetPuzzle(puzzleNum);
        if (Puzzle != null) Puzzle.Voxels.Add(this);
    }
}