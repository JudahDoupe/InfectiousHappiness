using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic.Framework;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Level{

        /* Voxels */
        public const int Size = 50;
        public Voxel StartingVoxel;
        public Vector3 WorldPostition = new Vector3(0,0,0);

        private readonly Voxel[,,] _voxels = new Voxel[Size, Size, Size];

        /* Constructors */
        public Level(Vector3 position)
        {
            WorldPostition = position;
        }
        public Level(string path)
        {
            LoadOriginal();
        }

        public void Save()
        {
            return;
        }
        public void LoadLastSave()
        {
            return;
        }
        public void LoadOriginal()
        {
            return;
        }

        /* Voxels */
        public int Blocks;
        public int ActiveBlocks;
        public Voxel GetVoxel(Vector3 localPos)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, Size-1));
            var y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, Size-1));
            var z = Mathf.RoundToInt(Mathf.Clamp(localPos.z, 0, Size-1));

            var rtn = _voxels[x, y, z];
            if (rtn != null) return rtn;

            var newPos = new Vector3(x, y, z);
            return SetVoxel(newPos, new Voxel { Position = newPos + WorldPostition });
        }
        public Voxel SetVoxel(Vector3 localPos, Voxel newVox)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, Size-1));
            var y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, Size-1));
            var z = Mathf.RoundToInt(Mathf.Clamp(localPos.z, 0, Size-1));

            if(_voxels[x, y, z] != null)
                _voxels[x, y, z].Destroy();
            _voxels[x, y, z] = newVox;
            return _voxels[x, y, z];
        }
        public Voxel PlaceNewBlock(Vector3 localPos, GameObject prefab)
        {
            var vox = GetVoxel(localPos);
            vox.Destroy();
            var obj = Object.Instantiate(prefab, vox.Position, Quaternion.identity);
            vox.Fill(obj);
            Blocks++;
            return vox;
        }
        public List<Block> GetAllBlocks()
        {
            return (from Voxel voxel in _voxels where voxel != null && voxel.HasBlock() && voxel.GetBlock().Type == BlockType.Static select voxel.GetBlock()).ToList();
        }

        /* General */
        public bool IsInsideMap(Vector3 localPos)
        {
            return 0 < localPos.x && localPos.x < Size
                   && 0 < localPos.y && localPos.y < Size
                   && 0 < localPos.z && localPos.z < Size;
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
