using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWell : Block
{
    public int SpringInterval = 1;

    private int _springTimer = 0;
    private Voxel _springVox;

    void Start()
    {
        Class = "Block";
        Type = "FireWell";
        _springVox = VoxelWorld.GetVoxel(transform.position + Vector3.up);
    }

    void FixedUpdate()
    {
        if (IsDyed && _springTimer == 0 && _springVox.Entity == null)
            _springVox.Fill(EntityConstructor.NewDroplet("Fire"));

        _springTimer = (_springTimer + 1) % (SpringInterval * 60);
    }
}