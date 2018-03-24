using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Droplet : Entity, IMovable {
    public new const string Name = "Droplet";
    public new const string Type = "Dye";
    public const int SplashRadius = 3;

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
