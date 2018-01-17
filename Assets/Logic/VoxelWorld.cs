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
        
        //Setup Active Level 
        ActiveLevel = new Level(LevelData.Name);
        if (!LevelData.ResetToTemplate)
            ActiveLevel.Load();

        if (!ActiveLevel.IsLoaded && LevelData.LevelTemplate != null)
            LevelData.LevelTemplate.Build(ActiveLevel);

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

    // Levels
    public LevelLookupData LevelData;
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
    public static bool IsInsideWorld(Vector3 worldPos)
    {
        return ActiveLevel.IsInsideLevel(worldPos);
    }
}

public class Level
{
    public const int NumRooms = 8;
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
    public void SaveAll()
    {
        for (int i = 0; i < NumRooms; i++)
        {
            GetRoom(i).Save();
        }
    }
    public void Save()
    {
        var saveData = new LevelData2
        {
            Name = Name,
            Pos = WorldPostition,
            SpawnPos = SpawnVoxel == null ? new Vector3(0, 0, 0) : SpawnVoxel.WorldPosition,

            PlayerCanJump = VoxelWorld.Instance.MainCharacter.CanJump,
            PlayerCanPush = VoxelWorld.Instance.MainCharacter.CanPush,
            PlayerCanLift = VoxelWorld.Instance.MainCharacter.CanLift,
            PlayerCanPipe = VoxelWorld.Instance.MainCharacter.CanPipe,
            PlayerCanSwitch = VoxelWorld.Instance.MainCharacter.CanSwitch,
        };
        IOManager.SaveLevel(saveData, Name);
    }
    public void Load()
    {
        if(IsLoaded) return;
        var savedData = IOManager.LoadLevel(Name);
        if (savedData.Name == null) return;

        WorldPostition = savedData.Pos;
        SpawnVoxel = GetVoxel(WorldToLevel(savedData.SpawnPos));

        var player = VoxelWorld.Instance.MainCharacter;
        player.CanJump = savedData.PlayerCanJump;
        player.CanPush = savedData.PlayerCanPush;
        player.CanLift = savedData.PlayerCanLift;
        player.CanPipe = savedData.PlayerCanPipe;
        player.CanSwitch = savedData.PlayerCanSwitch;

        for (int i = 0; i < NumRooms; i++)
        {
            GetRoom(i).Reload();
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
        if (roomNum < 0 || roomNum >= NumRooms) return null;
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
    public Vector3 RoomOffset = new Vector3(0,0,0);

    public List<Voxel> Voxels = new List<Voxel>();
    public List<Block> Blocks = new List<Block>();
    public List<Upgrade> Upgrades = new List<Upgrade>();

    public Room(Level level, int roomNum)
    {
        Level = level;
        RoomNumber = roomNum;
    }
    public void Load(RoomData2 data)
    {
        Destroy();

        Name = data.Name;
        RoomOffset = data.RoomOffset;

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
                var vox = Level.GetVoxel(voxData.LvlPos + RoomOffset);
                vox.Load(voxData, RoomNumber);
            }
        }
    }
    public void Reload()
    {
        var data = IOManager.LoadRoom(Level.Name, RoomNumber);
        Load(data);
    }
    public void Save()
    {
        List<VoxelData2> voxelsInRoom = new List<VoxelData2>();
        for (var i = 0; i < Blocks.Count; i++)
        {
            var block = Blocks[i];
            if (block == null) continue;

            var worldPos = block.Movement == null ? block.transform.position : block.Movement.SpawnVoxel.WorldPosition;
            voxelsInRoom.Add(new VoxelData2
            {
                Active = block.IsActivated && VoxelWorld.Instance.SaveProgress,
                Object = "Block",
                ObjectType = block.Type.ToString(),
                LvlPos = Level.WorldToLevel(worldPos - RoomOffset),
            });
        }
        for (var i = 0; i < Upgrades.Count; i++)
        {
            var upgrade = Upgrades[i];
            if (upgrade != null)
            {
                voxelsInRoom.Add(new VoxelData2
                {
                    Active = false,
                    Object = "Upgrade",
                    ObjectType = upgrade.Type.ToString(),
                    LvlPos = Level.WorldToLevel(upgrade.transform.position - RoomOffset),
                });
            }
        }

        IOManager.SaveRoom(new RoomData2
        {
            LevelName = Level.Name,
            Name = Name,
            RoomNum = RoomNumber,
            RoomOffset = RoomOffset,
            TrackName = Track == null ? "" : Track.name,
            Voxels = voxelsInRoom.ToArray(),
        });

        Level.Save();

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

    public void CompleteRoom()
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
    public Room Room;
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
    public void Load(VoxelData data)
    {
        WorldPosition = data.WorldPosition;

        var prefab = IOManager.LoadObject(data.ObjectName);
        GameObject obj = null;
        if (prefab != null)
            obj = UnityEngine.Object.Instantiate(prefab, WorldPosition, Quaternion.identity);

        Fill(obj, data.RoomNum);

        if (Block != null)
        {
            Block.SetType(data.ObjectName);
            if (data.IsActive)
                Block.Activate();
        }

        if (data.RoomNum >= 0) ChangeRoom(data.RoomNum);
    } //Depriciated
    public void Load(VoxelData2 data, int roomNum = -1)
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
    public GameObject TransferObject()
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
        if (Room != null) Room.RemoveVoxel(this);
        Room = Level.GetRoom(roomNum);
        if (Room != null) Room.AddVoxel(this);
    }
}