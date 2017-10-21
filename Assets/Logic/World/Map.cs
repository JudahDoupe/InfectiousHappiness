using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;

namespace Assets.Logic.World
{
    public static class Map
    {
        public const int Length = 100;
        public const int Width = 100;
        public const int Height = 50;

        public static int Score = 0;
        public static float Gravity = 0.01f;

        private static readonly Voxel[,,] Voxels = new Voxel[Width, Height, Length];
        private static readonly Dictionary<int,List<Block>> RegisteredBlocks = new Dictionary<int, List<Block>>();
        public static int TotalRegisteredBlocks;
        public static int TotalInfectedBlocks;

        private static int _infectionLevel = 0;

        public static Voxel StartingVoxel;

        public static Voxel GetVoxel(Vector3 pos)
        {
            var x = Mathf.RoundToInt(Mathf.Clamp(pos.x,0, Width));
            var y = Mathf.RoundToInt(Mathf.Clamp(pos.y, 0, Height));
            var z = Mathf.RoundToInt(Mathf.Clamp(pos.z, 0, Length));

            var rtn = Voxels[x, y, z];

            if (rtn != null) return rtn;

            SetVoxel(pos,new Voxel {Position = new Vector3(x,y,z)});
            rtn = Voxels[x, y, z];

            return rtn;
        }
        public static void SetVoxel(Vector3 pos, Voxel vox)
        {
            var x = Mathf.RoundToInt(pos.x);
            var y = Mathf.RoundToInt(pos.y);
            var z = Mathf.RoundToInt(pos.z);

            if (0 <= x && x <= Width &&
                0 <= y && y <= Height &&
                0 <= z && z <= Length)
                Voxels[x, y, z] = vox;
        }
        public static bool InsideMap(Vector3 pos)
        {
            return 0 < pos.x && pos.x < Width
                   && 0 < pos.y && pos.y < Height
                   && 0 < pos.z && pos.z < Length;
        }


        public static void RegisterBlock(Block block)
        {
            if (block.InfectionLevel == -1) return;

            if (!RegisteredBlocks.ContainsKey(block.InfectionLevel))
            {
                RegisteredBlocks.Add(block.InfectionLevel, new List<Block>());
            }

            var blocks = RegisteredBlocks[block.InfectionLevel];

            blocks.Add(block);
            TotalRegisteredBlocks++;
        }

        public static void InfectBlocksBelowLevel(int blockLevel)
        {
            var numBlocks = 0;
            while(_infectionLevel < blockLevel)
            {
                if (!RegisteredBlocks.ContainsKey(_infectionLevel))
                {
                    RegisteredBlocks.Add(_infectionLevel, new List<Block>());
                }

                foreach (var block in RegisteredBlocks[_infectionLevel])
                {
                    if (block.IsInfected) continue;

                    block.Infect();
                    numBlocks++;
                    TotalInfectedBlocks++;
                }
                Score += Mathf.RoundToInt(numBlocks * 0.25f);
                _infectionLevel++;
            }
        }
    }
}
