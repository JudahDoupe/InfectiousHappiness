using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Logic.Framework
{
    public class VoxelWorld : MonoBehaviour
    {
        void Awake(){ if (Instance == null) Instance = this;}

        void Update()
        {
            AudioSource baseTrack = null;
            if (ActiveLevel == null) return;

            for (var i = 0; i < 16; i++)
            {
                if (baseTrack == null) baseTrack = ActiveLevel.GetRoom(i).Track;

                ActiveLevel.GetRoom(i).Track.timeSamples = baseTrack.timeSamples;
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
        public static VoxelWorld Instance;
        public static Voxel SpawnVoxel
        {
            get { return ActiveLevel == null ? new Voxel(null, Vector3.zero) : ActiveLevel.SpawnVoxel; }
        }
        public static Vector3 GravityVector = new Vector3(0, -0.01f, 0);
        public bool PlayMusic = true;

        // Levels
        public static Level ActiveLevel;
        private static readonly List<Level> Levels = new List<Level>();
        public static Level GetLevel(Vector3 worldPos)
        {
            return Levels.FirstOrDefault(level => level.IsInsideLevel(worldPos));
        }
        public static Level AddLevel(Level level)
        {
            Levels.Add(level);
            return level;
        }
        public static bool RemoveLevel(Level level)
        {
            return Levels.Remove(level);
        }

        // Queries
        public static Voxel GetVoxel(Vector3 worldPos)
        {
            var level = GetLevel(worldPos);
            return level == null ? null : level.GetVoxel(level.WorldToLevel(worldPos));
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
        public const int Size = 48;
        public readonly Vector3 WorldPostition;
        public readonly Voxel SpawnVoxel;

        private readonly Room[] _rooms = new Room[16];
        private readonly Voxel[,,] _voxels = new Voxel[Size, Size, Size];

        /*
        public Level(string filePath)
        {
            filePath = Application.dataPath + filePath;

            string jsonData = File.ReadAllText(filePath);
            var data = JsonUtility.FromJson<LevelData>(jsonData);

            foreach (var roomData in data.Rooms)
            {
                GetRoom(roomData.RoomNum).BuildRoom(roomData);
            }
            foreach (var voxelData in data.Voxels)
            {
                GetVoxel(WorldToLevel(voxelData.Pos)).Fill(voxelData);
            }

            WorldPostition = data.Pos;
            SpawnVoxel = GetVoxel(WorldToLevel(data.SpawnPos));

        }
        */
        public Level(Vector3 worldPos, Vector3? spawnPos = null)
        {
            if(spawnPos == null) spawnPos = new Vector3(24, 1, 24);
            WorldPostition = worldPos;
            SpawnVoxel = GetVoxel(spawnPos.Value);
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

            if (!pos.All(x => x >= 0 && x < Size)) return null;

            return _voxels[pos[0], pos[1], pos[2]] ??
                   (_voxels[pos[0], pos[1], pos[2]] = new Voxel(this, new Vector3(pos[0], pos[1], pos[2]) + WorldPostition));
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

        //TODO: Fix everything about this
        public void ExportLevel(string filePath)
        {
            filePath = Application.dataPath + filePath;

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

            var data = new LevelData
            {
                Pos = WorldPostition,
                SpawnPos = SpawnVoxel.Position,

                Rooms = _rooms.Where(x => x != null).Select(x => x.ToData()).ToArray(),
                Voxels = voxels.ToArray(),
            };

            var json = JsonUtility.ToJson(data);
            File.WriteAllText(filePath, json);
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
                TrackPath = AssetDatabase.GetAssetPath(Track)
            };
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
            get{return Goals.Count > 0 || Goals.All(g => g.IsActivated);}
        }
        public void Complete()
        {
            if(IsComplete)
                VoxelWorld.Instance.StartCoroutine("RandomlyActivateBlocks", Floor);
        }
    }

    public class Voxel
    {
        public Vector3 Position { get; private set; }
        public Room Room { get; private set; }
        public Level Level;

        public GameObject Object { get; private set; }
        public Block Block { get { return Object == null ? null : Object.GetComponent<Block>(); } }
        public Character Character { get { return Object == null ? null : Object.GetComponent<Character>(); } }

        public Voxel(Level level, Vector3 position)
        {
            Level = level;
            Position = position;
            Room = null;
        }
        public VoxelData ToData()
        {
            var path = "";
            if (Block)
            {
                switch (Block.Type)
                {
                    case BlockType.Floor:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.FloorBlock);
                        break;
                    case BlockType.Goal:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.GoalBlock);
                        break;
                    case BlockType.Movable:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.MovableBlock);
                        break;
                    case BlockType.Bounce:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.BounceBlock);
                        break;
                    case BlockType.Pipe:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.PipeBlock);
                        break;
                    case BlockType.Switch:
                        path = AssetDatabase.GetAssetPath(VoxelWorld.Instance.SwitchBlock);
                        break;
                    default:
                        path = "";
                        break;
                }   
            }

            var data = new VoxelData
            {
                Pos = Position,
                RoomNum = Room == null ? -1 : Room.RoomNumber,
                PrefabPath = path
            };

            return data;
        }

        public GameObject Fill(VoxelData data)
        {
            Room = Level.GetRoom(data.RoomNum);
            if(Room != null)Room.AddVoxel(this);

            if (data.PrefabPath == null) return null;

            var prefab = AssetDatabase.LoadAssetAtPath(data.PrefabPath, typeof(GameObject)) as GameObject;

            if(prefab == null) return null;

            var obj = UnityEngine.Object.Instantiate(prefab, Position, Quaternion.identity);
            return Fill(obj);
        }
        public GameObject Fill(GameObject obj, int? roomNum = null)
        {

            if (obj == Object) return obj;

            DestroyObject();

            obj.transform.position = Position;
            Object = obj;

            if(roomNum != null)ChangeRoom(roomNum);

            return obj;
        }
        public GameObject TransferObject()
        {
            ChangeRoom(null);
            var obj = Object;
            Object = null;
            return obj;
        }
        public void DestroyObject()
        {
            ChangeRoom(null);
            if (Object != null) UnityEngine.Object.Destroy(Object);
            Object = null;
        }

        private void ChangeRoom(int? roomNum)
        {
            if (Room != null) Room.RemoveVoxel(this);
            if (roomNum == null)
            {
                Room = null;
                return;
            }
            Room = Level.GetRoom(roomNum.Value);
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
        public string PrefabPath;
    }

}
