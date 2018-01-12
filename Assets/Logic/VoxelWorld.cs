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

        foreach (var data in LevelData)
        {
            var level = new Level(data.Name);
            Levels.Add(level);

            if (data.ResetToTemplate && data.LevelTemplate != null)
                data.LevelTemplate.Build(level);
            else
                level.BuildLevelFromFile();
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
        return Levels.FirstOrDefault(level => level.IsInsideLevel(worldPos));
    }
    public static Level GetLevel(int levelNum)
    {
        return Levels.ElementAt(levelNum);
    }

    // Queries
    public static Voxel GetVoxel(Vector3 worldPos)
    {
        var level = GetLevel(worldPos);
        if (level != null) return level.GetVoxel(level.WorldToLevel(worldPos));
        //Unable to get voxel because level does not exist at this location
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
        ActiveLevel.SaveLevel();
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
    public string Name;
    public const int Size = 48;
    public Vector3 WorldPostition;
    public Voxel SpawnVoxel;
    public bool IsLoaded = true;

    private readonly Room[] _rooms = new Room[16];
    private readonly Voxel[,,] _voxels = new Voxel[Size, Size, Size];

    private LevelData _storedData;
    private readonly string _filePath;

    public Level(string name)
    {
        Name = name;
        _filePath = Application.persistentDataPath + "/LevelData/" + name + ".json";
        if (File.Exists(_filePath))
        {
            string jsonData = File.ReadAllText(_filePath);
            _storedData = JsonUtility.FromJson<LevelData>(jsonData);
        }
        else
        {
            SaveLevel();
        }

        WorldPostition = _storedData.Pos;
        SpawnVoxel = GetVoxel(WorldToLevel(_storedData.SpawnPos));
    }
    public Level(string name, Vector3 worldPos, Vector3? spawnPos = null)
    {
        Name = name;
        _filePath = Application.persistentDataPath + "/LevelData/" + name + ".json";
        if (File.Exists(_filePath))
        {
            string jsonData = File.ReadAllText(_filePath);
            _storedData = JsonUtility.FromJson<LevelData>(jsonData);
        }
        else
        {
            SaveLevel();
        }
        if (spawnPos == null) spawnPos = new Vector3(24, 1, 24);
        WorldPostition = worldPos;
        SpawnVoxel = GetVoxel(spawnPos.Value);
    }
    public void BuildLevelFromFile()
    {
        foreach (var roomData in _storedData.Rooms)
        {
            GetRoom(roomData.RoomNum).BuildRoom(roomData);
        }
        foreach (var voxelData in _storedData.Voxels)
        {
            GetVoxel(WorldToLevel(voxelData.Pos)).Fill(voxelData);
        }
        IsLoaded = true;
    }
    public void DestroyLevel()
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
            room.DestroyRoom();
        }
        IsLoaded = false;
    }
    public void SaveLevel()
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

        _storedData = new LevelData
        {
            Pos = WorldPostition,
            SpawnPos = SpawnVoxel == null ? new Vector3(0,0,0) : SpawnVoxel.Position,

            Rooms = _rooms.Where(x => x != null).Select(x => x.ToData()).ToArray(),
            Voxels = voxels.ToArray(),
        };

        var json = JsonUtility.ToJson(_storedData);
        if (!Directory.Exists(Application.persistentDataPath + "/LevelData"))
            Directory.CreateDirectory(Application.persistentDataPath + "/LevelData");
        File.WriteAllText(_filePath, json);

        Debug.Log("Saving Level");
    }

    public Room GetRoom(Vector3 levelPos)
    {
        return GetVoxel(levelPos).Room;
    }
    public Room GetRoom(int roomNum)
    {
        if (roomNum < 0 || roomNum >= 16) return null;
        return _rooms[roomNum] ?? (_rooms[roomNum] = new Room(roomNum));
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
        {
            return null;
        }

        return _voxels[pos[0], pos[1], pos[2]] ??
                (_voxels[pos[0], pos[1], pos[2]] = new Voxel(this, LevelToWorld(new Vector3(pos[0], pos[1], pos[2]))));
    }

    public bool IsInsideLevel(Vector3 worldPos)
    {
        var localPos = worldPos - WorldPostition;
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
    public AudioSource Track { get; private set; }
    public int RoomNumber { get; private set; }

    public List<Block> Floor = new List<Block>();
    public List<Block> Goals = new List<Block>();

    public Room(int roomNum)
    {
        RoomNumber = roomNum;
    }
    public RoomData ToData()
    {
        return new RoomData
        {
            RoomNum = RoomNumber,
            TrackPath = Track == null ? "" : Track.name
        };
    }
    public void DestroyRoom()
    {
        Floor = new List<Block>();
        Goals = new List<Block>();
        Object.Destroy(Track);
    }

    public void BuildRoom(RoomData data)
    {
        var clip = Resources.Load<AudioClip>(data.TrackPath);
        BuildRoom(clip);
    }
    public void BuildRoom(AudioClip clip)
    {
        Track = VoxelWorld.Instance.gameObject.AddComponent<AudioSource>();
        Track.clip = clip;
        Track.volume = 0;
    }

    public void AddVoxel(Voxel vox)
    {
        if (vox.Block != null)
        {
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
    }
    public void RemoveVoxel(Voxel vox)
    {
        if (vox.Block)
        {
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
    }

    public bool IsComplete
    {
        get{return Goals.Count > 0 && Goals.All(g => g.IsActivated);}
    }
    public void Complete()
    {
        if (IsComplete)
        {
            VoxelWorld.Instance.StartCoroutine("RandomlyActivateBlocks", Floor);
        }
    }
}

public class Voxel
{
    private int _RoomNum = -1;

    public Vector3 Position { get; private set; }
    public Room Room
    {
        get { return Level.GetRoom(_RoomNum); }
    }

    public Level Level;

    public GameObject Object { get; private set; }
    public Block Block { get { return Object == null ? null : Object.GetComponent<Block>(); } }
    public Character Character { get { return Object == null ? null : Object.GetComponent<Character>(); } }

    public Voxel(Level level, Vector3 position)
    {
        Level = level;
        Position = position;
    }
    public VoxelData ToData()
    {
        var name = "";
        var isActive = false;
        if (Block)
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
            Pos = Position,
            RoomNum = Room == null ? -1 : Room.RoomNumber,
            Name = name,
            IsActive = isActive
        };

        return data;
    }

    public GameObject Fill(VoxelData data)
    {
        if (data.Name == null) return null;
        var prefab = Resources.Load<GameObject>(data.Name);
        if (prefab == null) return null;

        var obj = UnityEngine.Object.Instantiate(prefab, Position, Quaternion.identity);
        Fill(obj, data.RoomNum);

        if (Block != null && data.IsActive)
        {
            switch (Block.Type)
            {
                case BlockType.Floor:
                    Block.FloorActivate(null);
                    break;
                case BlockType.Goal:
                    Block.GoalActivate(null);
                    break;
                default:
                    Block.IsActivated = true;
                    break;
            }
        }

        return Object;
    }
    public GameObject Fill(GameObject obj, int roomNum = -1)
    {

        if (obj == Object) return obj;
        if (roomNum < 0) roomNum = _RoomNum;

        DestroyObject();

        obj.transform.position = Position;
        Object = obj;

        ChangeRoom(roomNum);

        return obj;
    }
    public GameObject TransferObject()
    {
        ChangeRoom(-1);
        var obj = Object;
        Object = null;
        return obj;
    }
    public void DestroyObject()
    {
        ChangeRoom(-1);
        if (Object != null) UnityEngine.Object.Destroy(Object);
        Object = null;
    }

    private void ChangeRoom(int roomNum)
    {
        if (Room != null) Room.RemoveVoxel(this);
        _RoomNum = roomNum;
        if (Room != null) Room.AddVoxel(this);
    }
}

[Serializable]
public struct LevelData
{
    public Vector3 Pos;
    public Vector3 SpawnPos;

    public RoomData[] Rooms;
    public VoxelData[] Voxels;
}
[Serializable]
public struct RoomData
{
    public int RoomNum;
    public string TrackPath;
}
[Serializable]
public struct VoxelData
{
    public Vector3 Pos;
    public int RoomNum;
    public string Name;
    public bool IsActive;
}