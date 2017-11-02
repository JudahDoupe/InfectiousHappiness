using System.Collections.Generic;
using System.Linq;
using Assets.Logic.Framework;
using UnityEngine;

namespace Assets.Logic
{
    public static class World
    {
        // Properties
        public static Voxel SpawnVoxel
        {
            get { return CurrentLevel == null ? new Voxel { Position = Vector3.zero } : CurrentLevel.SpawnVoxel; }
        }
        public static Vector3 GravityVector = new Vector3(0, -0.01f, 0);

        // Levels
        private static readonly List<Level> _levels = new List<Level>();
        public static Level CurrentLevel
        {
            get { return _levels.First(); }
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
            return CurrentLevel == null ? null : CurrentLevel.GetVoxel(worldPos - CurrentLevel.WorldPostition);
        }
        public static bool IsInsideWorld(Vector3 worldPos)
        {
            if (CurrentLevel == null)
                return false;

            var localPos = worldPos - CurrentLevel.WorldPostition;
            return 0 < localPos.x && localPos.x < Level.Size
                   && 0 < localPos.y && localPos.y < Level.Size
                   && 0 < localPos.z && localPos.z < Level.Size;
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
        public Room CurrentRoom;
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
    }

    public class Room
    {
        private readonly Level _level;
        private AudioSource _track;
        public Vector3 LevelPostition;
        public bool IsComplete
        {
            get { return Goals.All(g => g.IsActivated); }
        }
        
        public List<Block> Floor = new List<Block>();
        public List<Block> Goals = new List<Block>();

        public Room(Vector3 position, Level level)
        {
            LevelPostition = position;
            _level = level;
        }

        public Voxel FillVoxel(Vector3 roomPos, GameObject prefab)
        {
            var vox = _level.GetVoxel(roomPos + LevelPostition);
            var obj = Object.Instantiate(prefab, vox.Position, Quaternion.identity);
            vox.Fill(obj);

            var block = obj.GetComponent<Block>();
            if (block)
            {
                switch (block.Type)
                {
                    case BlockType.Static:
                        Floor.Add(block);
                        break;
                    case BlockType.Goal:
                        Goals.Add(block);
                        break;
                }
            }

            return vox;
        }
        public void AssignTrack(AudioSource track)
        {
            _track = track;
            Async.Instance.AdjustTrackVolume(this,_track);
        }
        public void CompleteRoom()
        {
            if (!IsComplete) return;

            Async.Instance.RandomlyActivateBlocks(Floor);
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
