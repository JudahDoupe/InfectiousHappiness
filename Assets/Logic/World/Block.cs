using System.Collections.Generic;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Block : MonoBehaviour
    {
        public int InfectionLevel;
        public bool IsInfected;
        public bool IsBouncy;
        public bool IsGoal;

        public Material InfectedMaterial;

        void Update()
        {
            if (IsGoal)
            {
                var topBlock = Map.GetVoxel(transform.position + Vector3.up).Block;
                if (topBlock && topBlock.GetComponent<Movable>())
                    Map.InfectBlocksBelowLevel(InfectionLevel);
            }
        }

        public bool Infect()
        {
            if (IsInfected) return false;

            IsInfected = true;
            Map.Score++;
            Map.InfectBlocksBelowLevel(InfectionLevel);

            gameObject.GetComponentInChildren<MeshRenderer>().material = InfectedMaterial;

            return true;
        }
    }
}
