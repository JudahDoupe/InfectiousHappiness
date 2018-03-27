using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWell : Well
{
    void Start()
    {
        Class = "Block";
        Type = "WaterWell";
        DropletType = "Water";
        SpringVox = VoxelWorld.GetVoxel(transform.position + Vector3.up);
    }

    void FixedUpdate()
    {
        UpdateSpring();
    }
}