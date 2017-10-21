using System.Collections.Generic;
using UnityEngine;

namespace Assets.Logic.World
{
    public class Block : MonoBehaviour
    {
        public int InfectionLevel;
        public bool IsInfected;

        public Material InfectedMaterial;

        public Vector3 Top
        {
            get { return transform.position + transform.up; }
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
