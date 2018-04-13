using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Entity
{
    void Start()
    {
        Class = "Block";
        UpdateMaterial();
    }

    public virtual void Collide(Droplet droplet)
    {
        Destroy(droplet.gameObject);
    }
}
