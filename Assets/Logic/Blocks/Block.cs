using UnityEngine;
using System.Collections.Generic;

namespace Assets.Logic.Blocks
{
    public class Block : MonoBehaviour
    {
        public bool IsInfected;
        public List<Block> Chainedblocks;

        public Dictionary<Direction,Block> Neighbors;

        public Vector3 Top
        {
            get { return transform.position + transform.up; }
        }

        public bool Infect()
        {
            if (IsInfected) return false;

            IsInfected = true;
            foreach (var chainedblock in Chainedblocks)
            {
                chainedblock.Infect();
            }
            return true;
        }
    }
}
