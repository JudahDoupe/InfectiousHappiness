using System.Collections.Generic;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Block : MonoBehaviour
    {
        public int InfectionLevel;
        public bool IsInfected;
        public Dictionary<Vector3,Block> Neighbors = new Dictionary<Vector3, Block>();

        public Vector3 Top
        {
            get { return transform.position + transform.up; }
        }

        public bool Infect()
        {
            if (IsInfected) return false;

            IsInfected = true;
            World.Score++;
            World.InfectBlocksBelowLevel(InfectionLevel);

            return true;
        }

        public Block GetNeighbor(Vector3 direction)
        {
            var f = Vector3.Distance(direction, Vector3.forward);
            var b = Vector3.Distance(direction, Vector3.back);
            var l = Vector3.Distance(direction, Vector3.left);
            var r = Vector3.Distance(direction, Vector3.right);

            if (f < b && f < l && f < r)
                return Neighbors.ContainsKey(Vector3.forward) ? Neighbors[Vector3.forward] : null;
            if (b < l && b < r)
                return Neighbors.ContainsKey(Vector3.back) ? Neighbors[Vector3.back] : null;
            if (l < r)
                return Neighbors.ContainsKey(Vector3.left) ? Neighbors[Vector3.left] : null;

            return Neighbors.ContainsKey(Vector3.right) ? Neighbors[Vector3.right] : null;
        }
    }
}
