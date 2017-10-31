using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;
using System;
using System.Security.AccessControl;

namespace Assets.Logic.World
{
    public static class Map
    {
        public static List<Level> Levels = new List<Level>();
        public static Level CurrentLevel;

        public static Voxel StartingVoxel
        {
            get { return CurrentLevel == null ? new Voxel {Position = Vector3.zero} : CurrentLevel.StartingVoxel; }
        }

        /* Gravity */
        public static Vector3 GravityDirection = new Vector3(0, -1, 0);
        public static float GravityMultiplier = 0.01f;
        public static Vector3 GravityVector
        {
            get { return GravityDirection * GravityMultiplier; }
        }

        public static Voxel GetVoxel(Vector3 worldPos)
        {
            return CurrentLevel == null ? null : CurrentLevel.GetVoxel(worldPos - CurrentLevel.WorldPostition);
        }

        public static bool IsInsideMap(Vector3 worldPos)
        {
            if (CurrentLevel == null)
                return false;

            var localPos = worldPos - CurrentLevel.WorldPostition;
            return 0 < localPos.x && localPos.x < Level.Size
                   && 0 < localPos.y && localPos.y < Level.Size
                   && 0 < localPos.z && localPos.z < Level.Size;
        }
    }
}
