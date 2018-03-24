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

    public void Dye()
    {
        transform.Find("Model").GetComponent<Renderer>().material = DyeMaterial;
    }
}
