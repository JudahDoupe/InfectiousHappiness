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
        var topVox = VoxelWorld.GetVoxel((droplet.transform.position - transform.position).normalized + transform.position);
        if (topVox.Entity == null)
            topVox.Fill(droplet);
        else
            Destroy(droplet.gameObject);
    }
}
