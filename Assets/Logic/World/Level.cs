using System.Collections;
using System.Collections.Generic;
using Assets.Logic.Framework;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Level{

        /* Gravity */
        public Vector3 GravityDirection = new Vector3(0, -1, 0);
        public float GravityMultiplier = 0.01f;
        public Vector3 GravityVector
        {
            get { return GravityDirection * GravityMultiplier; }
        }

        /* Voxels */
        public const int Size = 50;
        public Voxel StartingVoxel;

        private Vector3 _worldPostition = new Vector3(0,0,0);
        private Voxel[,,] _voxels = new Voxel[Size, Size, Size];

        /* Constructors */
        public Level(Vector3 position)
        {
            _worldPostition = position;
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
        public Voxel GetVoxel(Vector3 pos)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(pos.x, 0, Size));
            var y = Mathf.RoundToInt(Mathf.Clamp(pos.y, 0, Size));
            var z = Mathf.RoundToInt(Mathf.Clamp(pos.z, 0, Size));

            var rtn = _voxels[x, y, z];
            if (rtn != null) return rtn;

            var newPos = new Vector3(x, y, z);
            return SetVoxel(newPos, new Voxel { Position = newPos });
        }
        public Voxel SetVoxel(Vector3 pos, Voxel newVox)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(pos.x, 0, Size));
            var y = Mathf.RoundToInt(Mathf.Clamp(pos.y, 0, Size));
            var z = Mathf.RoundToInt(Mathf.Clamp(pos.z, 0, Size));

            _voxels[x, y, z].Destroy();
            _voxels[x, y, z] = newVox;
            return _voxels[x, y, z];
        }
        public Voxel PlaceNewBlock(Vector3 pos, GameObject prefab)
        {
            var vox = GetVoxel(pos);
            vox.Destroy();
            var obj = UnityEngine.Object.Instantiate(prefab, vox.Position, Quaternion.identity);
            vox.Fill(obj);
            Blocks++;
            return vox;
        }
        public void ActivateAllBlocks()
        {
            foreach (var voxel in _voxels)
            {
                if (voxel.HasBlock())
                    voxel.GetBlock().Activate();
            }
        }

        /* General */
        public bool IsInsideMap(Vector3 pos)
        {
            return 0 < pos.x && pos.x < Size
                   && 0 < pos.y && pos.y < Size
                   && 0 < pos.z && pos.z < Size;
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
