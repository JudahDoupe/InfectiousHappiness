using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;
using System;

namespace Assets.Logic.World
{
    public static class Map
    {
        public const int Length = 100;
        public const int Width = 100;
        public const int Height = 50;

        public static int Score = 0;
        public static int InfectionLevel = 0;

        public static int TotalRegisteredBlocks;
        public static int TotalInfectedBlocks;

        public static Voxel StartingVoxel;
        public static Vector3 GravityDirection = new Vector3(0,-1,0);
        public static float GravityMultiplier = 0.01f;
        public static Vector3 GravityVector
        {
            get { return GravityDirection * GravityMultiplier; }
        }


        private static Voxel[,,] _voxels = new Voxel[Width, Height, Length];
        private static Dictionary<int,List<Block>> _registeredBlocks = new Dictionary<int, List<Block>>();

        /* Map */
        public static void Reset()
        {
            _voxels = new Voxel[Width, Height, Length];
            _registeredBlocks = new Dictionary<int, List<Block>>();
            TotalRegisteredBlocks = 0;
            TotalInfectedBlocks = 0;
            InfectionLevel = 0;
        }
        public static bool IsInsideMap(Vector3 pos)
        {
            return 0 < pos.x && pos.x < Width
                   && 0 < pos.y && pos.y < Height
                   && 0 < pos.z && pos.z < Length;
        }

        /* Voxels */
        public static Voxel GetVoxel(Vector3 pos)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(pos.x,0, Width));
            var y = Mathf.RoundToInt(Mathf.Clamp(pos.y, 0, Height));
            var z = Mathf.RoundToInt(Mathf.Clamp(pos.z, 0, Length));

            var rtn = _voxels[x, y, z];
            if (rtn == null)
            {
                var newPos = new Vector3(x, y, z);
                rtn = SetVoxel(newPos, new Voxel { Position = newPos });
            }
            return rtn;
        }
        public static Voxel SetVoxel(Vector3 pos, Voxel newVox)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(pos.x, 0, Width));
            var y = Mathf.RoundToInt(Mathf.Clamp(pos.y, 0, Height));
            var z = Mathf.RoundToInt(Mathf.Clamp(pos.z, 0, Length));

            if (_voxels[x, y, z] != null)
                ClearVoxel(_voxels[x, y, z]);
            _voxels[x, y, z] = newVox;
            return _voxels[x, y, z];
        }
        public static Voxel ClearVoxel(Voxel vox)
        {
            if (!vox.Block) return vox;

            UnregisterBlock(vox.Block);
            UnityEngine.Object.Destroy(vox.Block.gameObject);
            vox.Block = null;

            return vox;
        }
        public static Voxel ClearVoxel(Vector3 pos)
        {
            return ClearVoxel(GetVoxel(pos));
        }

        /* Blocks */
        public static Voxel PlaceBlock(Vector3 pos, GameObject prefab, int infectionLevel)
        {
            var vox = ClearVoxel(pos);
            var obj = UnityEngine.Object.Instantiate(prefab, vox.Position, Quaternion.identity);
            vox.Block = obj.GetComponent<Block>();

            vox.Block.InfectionLevel = infectionLevel;
            RegisterBlock(vox.Block);

            return vox;
        }
        public static void RegisterBlock(Block block)
        {
            if (block.InfectionLevel == -1) return;

            if (!_registeredBlocks.ContainsKey(block.InfectionLevel))
                _registeredBlocks.Add(block.InfectionLevel, new List<Block>());

            _registeredBlocks[block.InfectionLevel].Add(block);

            TotalRegisteredBlocks++;
        }
        public static void UnregisterBlock(Block block)
        {
            if (!_registeredBlocks.ContainsKey(block.InfectionLevel) || !_registeredBlocks[block.InfectionLevel].Contains(block)) return;

            _registeredBlocks[block.InfectionLevel].Remove(block);

            TotalRegisteredBlocks--;
        }
        public static void InfectBlocksBelowLevel(int infectionLevel)
        {
            var numBlocks = 0;
            while(InfectionLevel < infectionLevel)
            {
                if (!_registeredBlocks.ContainsKey(InfectionLevel))
                {
                    _registeredBlocks.Add(InfectionLevel, new List<Block>());
                }

                foreach (var block in _registeredBlocks[InfectionLevel])
                {
                    if (block.IsInfected) continue;

                    block.Infect();
                    numBlocks++;
                    TotalInfectedBlocks++;
                }
                Score += Mathf.RoundToInt(numBlocks * 0.25f);
                InfectionLevel++;
            }
        }
    }

    public class Voxel
    {
        public Vector3 Position;
        public Block Block;
    }
}
