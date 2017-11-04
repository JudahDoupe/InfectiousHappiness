using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic.Framework;
using UnityEngine;

namespace Assets.Logic
{
    public class World : MonoBehaviour
    {
        void Awake()
        {
            if (Instance == null) Instance = this;
        }

        void Update()
        {
            var baseTrack = _tracks.FirstOrDefault();
            if (!baseTrack) return;

            foreach (var track in _tracks)
            {
                track.timeSamples = baseTrack.timeSamples;
            }
        }

        // Properties
        public bool PlayMusic = true;
        public static World Instance;
        public static Voxel SpawnVoxel
        {
            get { return ActiveLevel == null ? new Voxel { Position = Vector3.zero } : ActiveLevel.SpawnVoxel; }
        }
        public static Vector3 GravityVector = new Vector3(0, -0.01f, 0);

        // Music
        private static List<AudioSource> _tracks = new List<AudioSource>();
        public static AudioSource AddTrack(AudioClip track, Room room)
        {
            if (!Instance.PlayMusic) return null;

            var src = Instance.gameObject.AddComponent<AudioSource>();
            src.clip = track;
            src.loop = true;
            _tracks.Add(src);
            room.Track = src;
            src.Play();
            Instance.StartCoroutine("AdjustTrackVolume",room);
            return src;
        }

        // Levels
        private static readonly List<Level> _levels = new List<Level>();
        public static Level ActiveLevel;
        public static Level ActivateLevel(Vector3 worldPos)
        {
            var newLevel = GetLevel(worldPos);
            if (ActiveLevel != newLevel)
                ActiveLevel = newLevel;
            return newLevel;
        }
        public static Level GetLevel(Vector3 worldPos)
        {
            return _levels.FirstOrDefault(level => level.IsInsideLevel(worldPos));
        }
        public static Level AddLevel(Vector3 position)
        {
            var level = new Level(position);
            _levels.Add(level);
            return level;
        }
        public static bool RemoveLevel(Level level)
        {
            return _levels.Remove(level);
        }

        // Queries
        public static Voxel GetVoxel(Vector3 worldPos)
        {
            var level = GetLevel(worldPos);
            return level == null ? null : level.GetVoxel(worldPos - level.WorldPostition);
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
            var speed = 5;
            foreach (var block in blocks)
            {
                block.Activate();
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
        // Properties
        public const int Size = 48;
        public Voxel SpawnVoxel;
        public Vector3 WorldPostition;

        public Level(Vector3 position)
        {
            WorldPostition = position;
            SpawnVoxel = GetVoxel(new Vector3(24, 1, 24));
        }
        
        // Rooms
        private readonly List<Room> _rooms = new List<Room>();

        public Room GetRoom(Vector3 position)
        {
            return _rooms.FirstOrDefault();
        }
        public Room AddRoom(Vector3 position)
        {
            var room = new Room(position, this);
            _rooms.Add(room);
            return room;
        }
        public bool RemoveRoom(Room room)
        {
            return _rooms.Remove(room);
        }

        // Voxels
        private readonly Voxel[,,] _voxels = new Voxel[Size, Size, Size];
        public Voxel GetVoxel(Vector3 levelPos)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(levelPos.x, 0, Size-1));
            var y = Mathf.RoundToInt(Mathf.Clamp(levelPos.y, 0, Size-1));
            var z = Mathf.RoundToInt(Mathf.Clamp(levelPos.z, 0, Size-1));

            return _voxels[x, y, z] ??
                   (_voxels[x, y, z] = new Voxel {Position = new Vector3(x, y, z) + WorldPostition});
        }

        // Queries
        public bool IsInsideLevel(Vector3 worldPos)
        {
            var localPos = worldPos - WorldPostition;
            return 0 <= localPos.x && localPos.x <= Size - 1
                   && 0 <= localPos.y && localPos.y <= Size - 1
                   && 0 <= localPos.z && localPos.z <= Size - 1;
        }
    }

    public class Room
    {
        // Properties
        public AudioSource Track;
        public Vector3 LevelPostition;

        public List<Block> Floor = new List<Block>();
        public List<Block> Goals = new List<Block>();

        private readonly Level _level;
        private bool _finishedBuilding;
        
        public Room(Vector3 position, Level level)
        {
            LevelPostition = position;
            _level = level;
        }

        // Commands
        public Voxel FillVoxel(Vector3 roomPos, GameObject prefab)
        {
            var vox = _level.GetVoxel(roomPos + LevelPostition);
            var obj = Object.Instantiate(prefab, vox.Position, Quaternion.identity);
            vox.Fill(obj);

            var block = obj.GetComponent<Block>();
            if (block)
            {
                block.Room = this;
                switch (block.Type)
                {
                    case BlockType.Floor:
                        Floor.Add(block);
                        break;
                    case BlockType.Goal:
                        Goals.Add(block);
                        break;
                }
            }

            return vox;
        }
        public Voxel DestroyVoxel(Voxel vox)
        {
            if (vox.HasBlock())
            {
                switch (vox.GetBlock().Type)
                {
                    case BlockType.Floor:
                        Floor.Remove(vox.GetBlock());
                        break;
                    case BlockType.Goal:
                        Goals.Remove(vox.GetBlock());
                        break;
                }
            }

            return vox;
        }
        public void FinishBuilding()
        {
            _finishedBuilding = true;
        }
        public void CompleteRoom()
        {
            if (!IsComplete) return;
            World.Instance.StartCoroutine("RandomlyActivateBlocks", Floor);
        }
        
        // Queries
        public bool IsComplete
        {
            get { return _finishedBuilding && Goals.All(g => g.IsActivated); }
        }
    }

    public class Voxel
    {
        public Vector3 Position;
        private GameObject _obj;
        private Block _block;
        private Character _character;

        public GameObject Fill(GameObject obj)
        {
            Destroy();
            obj.transform.position = Position;
            _block = obj.GetComponent<Block>();
            _character = obj.GetComponent<Character>();
            _obj = obj;
            return obj;
        }
        public GameObject Empty()
        {
            var obj = _obj;
            _block = null;
            _character = null;
            _obj = null;
            return obj;
        }
        public void Destroy()
        {
            if(_obj != null)
                Object.Destroy(_obj);
            if (_block != null && _block.Room != null)
                _block.Room.DestroyVoxel(this);
            _block = null;
            _obj = null;
            _character = null;
        }

        public bool IsEmpty()
        {
            return _obj == null;
        }

        public bool HasBlock()
        {
            return _block != null;
        }
        public Block GetBlock()
        {
            return _block;
        }

        public bool HasCharacter()
        {
            return _character != null;
        }
        public Character GetCharacter()
        {
            return _character;
        }

        public GameObject GetObject()
        {
            return _obj;
        }
    }

}
