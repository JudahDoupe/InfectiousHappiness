using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DyeWell : Well
{
    void Start()
    {
        Class = "Block";
        Type = "DyeWell";
        DropletType = "Dye";
        SpringVox = VoxelWorld.GetVoxel(transform.position + Vector3.up);
    }

    void FixedUpdate()
    {
        UpdateSpring();
    }
}
