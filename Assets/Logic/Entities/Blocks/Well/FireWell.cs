using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWell : Well
{
    void Start()
    {
        Class = "Block";
        Type = "FireWell";
        DropletType = "Fire";
        SpringVox = VoxelWorld.GetVoxel(transform.position + Vector3.up);
    }

    void FixedUpdate()
    {
        UpdateSpring();
    }
}