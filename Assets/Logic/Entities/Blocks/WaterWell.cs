using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWell : Block
{
    public int SpringInterval = 1;

    private int _springTimer = 0;
    private Voxel _springVox;

    void Start()
    {
        Class = "Block";
        Type = "WaterWell";
        _springVox = VoxelWorld.GetVoxel(transform.position + Vector3.up);
    }

    void FixedUpdate()
    {
        if (IsDyed && _springTimer == 0 && _springVox.Entity == null)
            _springVox.Fill(EntityConstructor.NewDroplet("Water"));

        _springTimer = (_springTimer + 1) % (SpringInterval * 60);
    }
}