using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public string Class;
    [HideInInspector]
    public string Type;
    [HideInInspector]
    public int Variation;

    public Voxel Voxel;
}
