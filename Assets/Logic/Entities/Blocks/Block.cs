using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Entity
{
    public new const string Name = "Block";
    public new const string Type = "Static";

    public Material DyeMaterial;

    public void Dye()
    {
        transform.Find("Model").GetComponent<Renderer>().material = DyeMaterial;
    }
}
