using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Droplet : Entity, IMovable {

    public int SplashRadius = 3;

    void Start()
    {
        Class = "Droplet";
    }

    public virtual void Splash()
    {
        VoxelWorld.GetVoxel(transform.position).Fill(this);

        Voxel.Destroy();
    }

    public void Reset()
    {
        Splash();
    }
    public void Fall()
    {
        throw new NotImplementedException();
    }
    public void MoveTo(Voxel vox, bool forceMove = false)
    {
        throw new NotImplementedException();
    }
    public void MoveAlongPath(Voxel[] path, bool forceMove = true)
    {
        throw new NotImplementedException();
    }
    public void ArchTo(Voxel vox, bool forceMove = false)
    {
        throw new NotImplementedException();
    }
}
