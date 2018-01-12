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
    [Serializable]
    public struct LevelLookupData
    {
        public string Name;
        public bool ResetToTemplate;
        public LevelTemplate LevelTemplate;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (MainCharacter == null)
            MainCharacter = FindObjectOfType<Character>();

        for (var i = 0; i < LevelData.Count; i++)
        {
            var data = LevelData[i];
            var level = new Level(data.Name);
            Levels.Add(level);

            var activeLevel = ActiveLevel;
            if (activeLevel != null && activeLevel.Name == data.Name)
            {
                if(!data.ResetToTemplate)
                    activeLevel.Load();
                
                if (!activeLevel.IsLoaded && data.LevelTemplate != null)
                    data.LevelTemplate.Build(level);
            }

        }
    }
    void Update()
    {
        AudioSource baseTrack = null;

        for (var i = 0; i < 16; i++)
        {
            if (baseTrack == null) baseTrack = ActiveLevel.GetRoom(i).Track;

            var track = ActiveLevel.GetRoom(i).Track;
            if(track != null) track.timeSamples = baseTrack.timeSamples;
        }
    }

    // Blocks
    public GameObject FloorBlock;
    public GameObject MovableBlock;
    public GameObject GoalBlock;
    public GameObject BounceBlock;
    public GameObject PipeBlock;
    public GameObject SwitchBlock;

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

    // Levels
    public List<LevelLookupData> LevelData = new List<LevelLookupData>();
    private static readonly List<Level> Levels = new List<Level>();
    public static Level ActiveLevel
    {
        get{return GetLevel(Instance.MainCharacter.transform.position) ?? Levels.FirstOrDefault();}
    }
    public static Level GetLevel(Vector3 worldPos)
    {
        for (var i = 0; i < Levels.Count; i++)
        {
            var level = Levels[i];
            if (level.IsInsideLevel(worldPos))
                return level;
        }
        return null;
    }
    public static Level GetLevel(string name)
    {
        for (var i = 0; i < Levels.Count; i++)
        {
            if (Levels[i].Name == name)
                return Levels[i];
        }
        return null;
    }

    // Queries
    public static Voxel GetVoxel(Vector3 worldPos)
    {
        var level = GetLevel(worldPos);
        if (level != null) return level.GetVoxel(level.WorldToLevel(worldPos));
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
    public static bool IsInsideWorld(Vector3 worldPos)
    {
        return GetLevel(worldPos) != null;
    }

    // Coroutines
    public IEnumerator RandomlyActivateBlocks(List<Block> blocks)
    {
        blocks = blocks.OrderBy(x => Random.Range(0, 100)).ToList();
        var i = 0;
        const int speed = 5;
        foreach (var block in blocks)
        {
            block.Stand();
            if (i == 0)
                yield return new WaitForFixedUpdate();
            i = (i + 1) % speed;
        }
        ActiveLevel.Save();
    }
    public IEnumerator AdjustTrackVolume(Room room)
    {
        while (!room.IsComplete)
        {
            room.Track.volume = room.Floor.Count(x => x.IsActivated) / (room.Floor.Count + 0f);
            yield return new WaitForSeconds(1);
        }
        room.Track.volume = 1;
    }
}

public class Level
{
    public const int NumRooms = 16;
    public const int Size = 48;
    public string Name;
    public Vector3 WorldPostition;
    public Voxel SpawnVoxel;
    public bool IsLoaded;

    private readonly Room[] _rooms = new Room[NumRooms];
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
    public void Save()
    {

        var voxels = new List<VoxelData>();
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    if (_voxels[x,y,z] != null)
                    {
                        voxels.Add(_voxels[x,y,z].ToData());
                    }
                }
            }
        }

        var saveData = new LevelData
        {
            Name = Name,
            Pos = WorldPostition,
            SpawnPos = SpawnVoxel == null ? new Vector3(0,0,0) : SpawnVoxel.Position,

            Rooms = _rooms.Where(x => x != null).Select(x => x.ToData()).ToArray(),
            Voxels = voxels.ToArray(),
        };

        IOController.SaveLevel(saveData,Name);
    }
    public void Load()
    {
        if(IsLoaded) return;
        var savedData = IOController.LoadLevel(Name);
        if (savedData.Name != Name)
            return;

        WorldPostition = savedData.Pos;
        SpawnVoxel = GetVoxel(WorldToLevel(savedData.SpawnPos));

        foreach (var roomData in savedData.Rooms)
        {
            GetRoom(roomData.RoomNum).Load(roomData);
        }
        foreach (var voxelData in savedData.Voxels)
        {
            GetVoxel(WorldToLevel(voxelData.WorldPosition)).Load(voxelData);
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
        foreach (var room in _rooms)
        {
            room.Destroy();
        }
        IsLoaded = false;
    }

    public Room GetRoom(int roomNum)
    {
        if (roomNum < 0 || roomNum >= 16) return null;
        return _rooms[roomNum] ?? (_rooms[roomNum] = new Room(this,roomNum));
    }
    public Room GetRoom(Vector3 levelPos)
    {
        return GetVoxel(levelPos).Room;
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

    public bool IsInsideLevel(Vector3 worldPos)
    {
        var localPos = WorldToLevel(worldPos);
        return 0 <= localPos.x && localPos.x <= Size - 1
                && 0 <= localPos.y && localPos.y <= Size - 1
                && 0 <= localPos.z && localPos.z <= Size - 1;
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

public class Room
{
    public Level Level;
    public AudioSource Track;
    public int RoomNumber;
    public string Name;

    public List<Block> Floor = new List<Block>();
    public List<Block> Goals = new List<Block>();

    public Room(Level level, int roomNum)
    {
        Level = level;
        RoomNumber = roomNum;
    }
    public void Load(RoomData data)
    {
        Name = data.Name;

        if (Track != null || data.TrackName == "") return;

        Track = VoxelWorld.Instance.gameObject.AddComponent<AudioSource>();
        Track.clip = IOController.LoadTrack(data.TrackName);
        Track.volume = 0;
    }
    public RoomData ToData()
    {
        return new RoomData
        {
            Name = Name,
            RoomNum = RoomNumber,
            TrackName = Track == null ? "" : Track.name
        };
    }
    public void Destroy()
    {
        Floor = new List<Block>();
        Goals = new List<Block>();
        Object.Destroy(Track);
    }

    public void AddVoxel(Voxel vox)
    {
        if (vox.Block == null) return;

        switch (vox.Block.Type)
        {
            case BlockType.Floor:
                Floor.Add(vox.Block);
                break;
            case BlockType.Goal:
                Goals.Add(vox.Block);
                break;
        }
    }
    public void RemoveVoxel(Voxel vox)
    {
        if (!vox.Block) return;

        switch (vox.Block.Type)
        {
            case BlockType.Floor:
                Floor.Remove(vox.Block);
                break;
            case BlockType.Goal:
                Goals.Remove(vox.Block);
                break;
        }
    }

    public bool IsComplete
    {
        get
        {
            if (Goals.Count <= 0) return false;
            for (int i = 0; i < Goals.Count; i++)
            {
                if (!Goals[i].IsActivated) return false;
            }
            return true;
        }
    }
    public void Complete()
    {
        if (!IsComplete || !Level.IsLoaded) return;
        VoxelWorld.Instance.StartCoroutine("RandomlyActivateBlocks", Floor);
    }
}

public class Voxel
{
    public Level Level;
    public Room Room;
    public Vector3 Position;

    public GameObject Object;
    public Block Block;
    public Character Character;

    public Voxel(Level level, Vector3 position)
    {
        Level = level;
        Position = position;
    }
    public void Load(VoxelData data)
    {
        Position = data.WorldPosition;

        var prefab = IOController.LoadObject(data.ObjectName);
        GameObject obj = null;
        if (prefab != null)
            obj = UnityEngine.Object.Instantiate(prefab, Position, Quaternion.identity);

        Fill(obj, data.RoomNum);
        
        if (Block != null && data.IsActive)
        {
            switch (Block.Type)
            {
                case BlockType.Floor:
                    Block.FloorActivate();
                    break;
                case BlockType.Goal:
                    Block.GoalActivate();
                    break;
                default:
                    Block.IsActivated = true;
                    break;
            }
        }
    }
    public VoxelData ToData()
    {
        var name = "";
        var isActive = false;
        if (Block != null)
        {
            switch (Block.Type)
            {
                case BlockType.Floor:
                    name = VoxelWorld.Instance.FloorBlock.name;
                    break;
                case BlockType.Goal:
                    name = VoxelWorld.Instance.GoalBlock.name;
                    break;
                case BlockType.Movable:
                    name = VoxelWorld.Instance.MovableBlock.name;
                    break;
                case BlockType.Bounce:
                    name = VoxelWorld.Instance.BounceBlock.name;
                    break;
                case BlockType.Pipe:
                    name = VoxelWorld.Instance.PipeBlock.name;
                    break;
                case BlockType.Switch:
                    name = VoxelWorld.Instance.SwitchBlock.name;
                    break;
                default:
                    name = "";
                    break;
            }

            isActive = Block.IsActivated && VoxelWorld.Instance.SaveProgress;
        }

        var data = new VoxelData
        {
            WorldPosition = Position,
            RoomNum = Room == null ? -1 : Room.RoomNumber,
            ObjectName = name,
            IsActive = isActive
        };

        return data;
    }

    public void Fill(GameObject obj, int roomNum = -1)
    {
        DestroyObject();

        if (obj != null)
        {
            Object = obj;
            Object.transform.position = Position;
            Block = Object.GetComponent<Block>();
            Character = Object.GetComponent<Character>();
        }

        if(roomNum >= 0) ChangeRoom(roomNum);
    }
    public GameObject TransferObject()
    {
        var obj = Object;
        Object = null;
        Block = null;
        Character = null;
        return obj;
    }
    public void DestroyObject()
    {
        ChangeRoom(-1);
        if (Object != null && Character == null) UnityEngine.Object.Destroy(Object);
        Object = null;
        Block = null;
        Character = null;
    }

    private void ChangeRoom(int roomNum)
    {
        if (Room != null) Room.RemoveVoxel(this);
        Room = Level.GetRoom(roomNum);
        if (Room != null) Room.AddVoxel(this);
    }
}