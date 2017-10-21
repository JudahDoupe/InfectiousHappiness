using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Logic.World
{
    public static class Map
    {

        public static int Score = 0;

        private static readonly Voxel[,,] Voxels = new Voxel[25,10,25];
        private static readonly List<Block>[] RegisteredBlocks = new List<Block>[100];
        private static int _maxInfectionLevel = 0;

        public static Voxel StartingVexel;

        public static Voxel GetVoxel(Vector3 pos)
        {
            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);
            var z = Mathf.FloorToInt(pos.z);

            if (0 <= x && x <= 25 &&
                0 <= y && y <= 10 &&
                0 <= z && z <= 25)
            {
                var rtn = Voxels[x, y, z];

                if (rtn != null) return rtn;

                SetVoxel(pos,new Voxel {Position = new Vector3(x,y,z)});
                rtn = Voxels[x, y, z];

                return rtn;
            }

            return null;
        }
        public static void SetVoxel(Vector3 pos, Voxel vox)
        {
            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);
            var z = Mathf.FloorToInt(pos.z);

            if (0 <= x && x <= 25 &&
                0 <= y && y <= 10 &&
                0 <= z && z <= 25)
                Voxels[x, y, z] = vox;
        }

        public static void RegisterBlock(Block block)
        {
            var blocks = RegisteredBlocks[block.InfectionLevel];
            if (blocks == null)
            {
                RegisteredBlocks[block.InfectionLevel] = new List<Block>();
                blocks = RegisteredBlocks[block.InfectionLevel];
            }
            blocks.Add(block);
        }

        public static void InfectBlocksBelowLevel(int blockLevel)
        {
            var numBlocks = 0;
            for (var i = _maxInfectionLevel; i < blockLevel; i++)
            {
                var blocks = RegisteredBlocks[i];
                if (blocks == null)
                {
                    RegisteredBlocks[i] = new List<Block>();
                    continue;
                }

                foreach (var block in blocks)
                {
                    if (!block.Infect()) continue;

                    numBlocks++;
                    Score += numBlocks;
                }
            }
            if (blockLevel > _maxInfectionLevel)
                _maxInfectionLevel = blockLevel;
        }
    }
}
