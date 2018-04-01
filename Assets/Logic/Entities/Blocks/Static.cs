using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Static : Block {

    void Start()
    {
        Class = "Block";
        Type = "Static";
        UpdateMaterial();
    }
}
