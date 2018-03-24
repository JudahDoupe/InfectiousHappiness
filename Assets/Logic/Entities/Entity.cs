using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public string Class;
    [HideInInspector]
    public string Type;

    public Voxel Voxel;
}
