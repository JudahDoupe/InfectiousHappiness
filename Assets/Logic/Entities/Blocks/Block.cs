using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Entity
{
    void Start()
    {
        Class = "Block";
    }

    public Material DyeMaterial;
    public bool IsDyed;

    public void Dye()
    {
        transform.Find("Model").GetComponent<Renderer>().material = DyeMaterial;
        IsDyed = true;
    }
    public void Undye()
    {
        transform.Find("Model").GetComponent<Renderer>().material = EntityConstructor.Instance.UndyedBlockMaterial;
        IsDyed = false;
    }
}
