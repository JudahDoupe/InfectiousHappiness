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

    public virtual void Tap(Vector3 tapPos)
    {
        Debug.Log("Tapped "+Type+" "+Class+" at "+tapPos);
    }
}
