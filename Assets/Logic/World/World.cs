using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Logic.World
{
    public static class World
    {

        public static int Score = 0;

        private static readonly Voxel[,,] Voxels = new Voxel[25,10,25];
        private static readonly List<Block>[] RegisteredBlocks = new List<Block>[100];
        private static int _maxInfectionLevel = 0;

        public static Voxel GetVoxel(Vector3 pos)
        {
            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);
            var z = Mathf.FloorToInt(pos.z);

            if (0 < x && x < 25 &&
                0 < y && y < 10 &&
                0 < z && z < 25)
                return Voxels[x, y, z];

            return null;
        }
        public static void SetVoxel(Vector3 pos, Voxel vox)
        {
            var x = Mathf.FloorToInt(pos.x);
            var y = Mathf.FloorToInt(pos.y);
            var z = Mathf.FloorToInt(pos.z);

            if (0 < x && x < 25 &&
                0 < y && y < 10 &&
                0 < z && z < 25)
                Voxels[x, y, z] = vox;
        }

        public static void RegisterBlock(Block block)
        {
            RegisteredBlocks[block.InfectionLevel].Add(block);
        }

        public static void InfectBlocksBelowLevel(int blockLevel)
        {
            var numBlocks = 0;
            for (var i = _maxInfectionLevel; i < blockLevel; i++)
            {
                foreach (var block in RegisteredBlocks[i])
                {
                    numBlocks++;
                    block.Infect();
                    Score += numBlocks;
                }
            }
            if (blockLevel > _maxInfectionLevel)
                _maxInfectionLevel = blockLevel;
        }
    }
}
