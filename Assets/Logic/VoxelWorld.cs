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
            if (baseTrack == null) baseTrack = ActiveLevel.GetRoom(i).Track;

            var track = ActiveLevel.GetRoom(i).Track;
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
    public bool SaveProgress;
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

    private readonly Puzzle[] _puzzles = new Puzzle[NumPuzzles];
    private readonly Voxel[,,] _voxels = new Voxel[Size, Size, Size];

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
        Save();
        for (int i = 0; i < NumPuzzles; i++)
        {
            GetRoom(i).Save();
        }
    }
    public void Save()
    {
        var saveData = new LevelData
        {
            Name = Name,
            Position = WorldPostition,
            SpawnPosition = SpawnVoxel == null ? new Vector3(0, 0, 0) : SpawnVoxel.WorldPosition,
            PlayerCanJump = VoxelWorld.Instance.MainCharacter.CanJump,
            PlayerCanPush = VoxelWorld.Instance.MainCharacter.CanPush,
            PlayerCanLift = VoxelWorld.Instance.MainCharacter.CanLift,
            PlayerCanPipe = VoxelWorld.Instance.MainCharacter.CanPipe,
            PlayerCanSwitch = VoxelWorld.Instance.MainCharacter.CanSwitch,
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

        var player = VoxelWorld.Instance.MainCharacter;
        player.CanJump = data.PlayerCanJump;
        player.CanPush = data.PlayerCanPush;
        player.CanLift = data.PlayerCanLift;
        player.CanPipe = data.PlayerCanPipe;
        player.CanSwitch = data.PlayerCanSwitch;

        for (int i = 0; i < NumPuzzles; i++)
        {
            GetRoom(i).Reset();
        }
        IsLoaded = true;
    }
    public void Unload()
    {
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    if (_voxels[x, y, z] != null && _voxels[x, y, z].Character == null)
                    {
                        _voxels[x, y, z].DestroyObject();
                    }
                }
            }
        }
        foreach (var room in _puzzles)
        {
            room.Destroy();
        }
        IsLoaded = false;
    }

    public Puzzle GetRoom(int roomNum)
    {
        if (roomNum < 0 || roomNum >= NumPuzzles) return null;
        return _puzzles[roomNum] ?? (_puzzles[roomNum] = new Puzzle(this,roomNum));
    }
    public Puzzle GetRoom(Vector3 levelPos)
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

        return _voxels[pos[0], pos[1], pos[2]] ??
                (_voxels[pos[0], pos[1], pos[2]] = new Voxel(this, LevelToWorld(new Vector3(pos[0], pos[1], pos[2]))));
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
    public List<Block> Blocks = new List<Block>();
    public List<Upgrade> Upgrades = new List<Upgrade>();

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
        List<VoxelData> voxelsInRoom = new List<VoxelData>();
        for (var i = 0; i < Blocks.Count; i++)
        {
            var block = Blocks[i];
            if (block == null) continue;

            var worldPos = block.Movement == null ? block.transform.position : block.Movement.SpawnVoxel.WorldPosition;
            voxelsInRoom.Add(new VoxelData
            {
                Active = block.IsActivated && VoxelWorld.Instance.SaveProgress,
                Object = "Block",
                ObjectType = block.Type.ToString(),
                LvlPos = Level.WorldToLevel(worldPos - PuzzleOffset),
            });
        }
        for (var i = 0; i < Upgrades.Count; i++)
        {
            var upgrade = Upgrades[i];
            if (upgrade != null)
            {
                voxelsInRoom.Add(new VoxelData
                {
                    Active = false,
                    Object = "Upgrade",
                    ObjectType = upgrade.Type.ToString(),
                    LvlPos = Level.WorldToLevel(upgrade.transform.position - PuzzleOffset),
                });
            }
        }

        IOManager.SavePuzzle(new PuzzleData
        {
            LevelName = Level.Name,
            PuzzleName = Name,
            PuzzleNum = Number,
            PuzzleOffset = PuzzleOffset,
            TrackName = Track == null ? "" : Track.name,
            Voxels = voxelsInRoom.ToArray(),
        });
    }
    public void Destroy()
    {
        for (var i = 0; i < Voxels.Count; i++)
        {
            Voxels[i].DestroyObject();
        }
        Voxels = new List<Voxel>();

        for (var i = 0; i < Blocks.Count; i++)
        {
            if (Blocks[i] != null)
                Object.Destroy(Blocks[i].gameObject);
        }
        Blocks = new List<Block>();

        for (var i = 0; i < Upgrades.Count; i++)
        {
            if (Upgrades[i] != null)
                Object.Destroy(Upgrades[i].gameObject);
        }
        Upgrades = new List<Upgrade>();

        Name = "";
        Object.Destroy(Track);
    }

    public void AddVoxel(Voxel vox)
    {
        Voxels.Add(vox);

        if (vox.Block)
            Blocks.Add(vox.Block);
        else if(vox.Upgrade)
            Upgrades.Add(vox.Upgrade);
    }
    public void RemoveVoxel(Voxel vox)
    {
        Voxels.Remove(vox);

        if (vox.Block)
            Blocks.Remove(vox.Block);
        if (vox.Upgrade)
            Upgrades.Remove(vox.Upgrade);
    }

    public void CompletePuzzle()
    {
        if (Level.IsLoaded)
        {
            VoxelWorld.Instance.StartCoroutine(ActivateAllFloorBlocks());
            Level.SpawnVoxel = VoxelWorld.GetVoxel(VoxelWorld.Instance.MainCharacter.transform.position);
            Level.Save();
        }
    }
    private IEnumerator ActivateAllFloorBlocks()
    {
        var blocks = Blocks.Where(b => b.Type == BlockType.Floor).OrderBy(x => Random.Range(0, 100)).ToArray();
        var i = 0;
        const int speed = 5;
        for (var index = 0; index < blocks.Length; index++)
        {
            var block = blocks[index];
            block.Stand();
            if (i == 0)
                yield return new WaitForFixedUpdate();
            i = (i + 1) % speed;
        }
        Save();
    }
}

public class Voxel
{
    public Level Level;
    public Puzzle Puzzle;
    public Vector3 WorldPosition;

    public GameObject Object;
    public Block Block;
    public Upgrade Upgrade;
    public Character Character;

    public Voxel(Level level, Vector3 worldPosition)
    {
        Level = level;
        WorldPosition = worldPosition;
    }
    public void Load(VoxelData data, int roomNum = -1)
    {
        DestroyObject();

        var prefab = IOManager.LoadObject(data.Object);
        if (prefab != null)
        {
            Object = UnityEngine.Object.Instantiate(prefab, WorldPosition, Quaternion.identity);
            Block = Object.GetComponent<Block>();
            Upgrade = Object.GetComponent<Upgrade>();
            Character = Object.GetComponent<Character>();
        }

        if (Block != null)
        {
            Block.SetType(data.ObjectType);
            if (data.Active)
                Block.Activate();
        }
        if (Upgrade != null)
        {
            Upgrade.SetType(data.ObjectType);
        }

        if (roomNum >= 0) ChangeRoom(roomNum);
    }
    public void Fill(GameObject obj, int roomNum = -1)
    {
        DestroyObject();

        if (obj != null)
        {
            Object = obj;
            Object.transform.position = WorldPosition;
            Block = Object.GetComponent<Block>();
            Upgrade = Object.GetComponent<Upgrade>();
            Character = Object.GetComponent<Character>();
        }

        if(roomNum >= 0) ChangeRoom(roomNum);
    }
    public GameObject Empty()
    {
        var obj = Object;
        Object = null;
        Block = null;
        Upgrade = null;
        Character = null;
        return obj;
    }
    public void DestroyObject()
    {
        ChangeRoom(-1);

        if (Object != null && Character == null)
            UnityEngine.Object.Destroy(Object);

        Object = null;
        Block = null;
        Upgrade = null;
        Character = null;
    }

    private void ChangeRoom(int roomNum)
    {
        if (Puzzle != null) Puzzle.RemoveVoxel(this);
        Puzzle = Level.GetRoom(roomNum);
        if (Puzzle != null) Puzzle.AddVoxel(this);
    }
}