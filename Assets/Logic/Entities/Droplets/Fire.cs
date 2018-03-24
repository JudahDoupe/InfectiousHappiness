using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Droplet
{
    void Start()
    {
        Class = "Droplet";
        Type = "Fire";
        SplashRadius = 2;
    }

    public override void Splash()
    {
        VoxelWorld.GetVoxel(transform.position).Fill(this);

        var r = SplashRadius;
        for (int i = -r; i <= r; i++)
        {
            for (int j = -r; j <= r; j++)
            {
                for (int k = -r; k <= r; k++)
                {
                    var vox = VoxelWorld.GetVoxel(transform.position + Vector3.right * i + Vector3.up * j + Vector3.forward * k);
                    if (vox != Voxel && 
                        vox != null && 
                        vox.Entity is Block && 
                        Vector3.Distance(Vector3.zero, new Vector3(i, j, k)) < r)
                        vox.Destroy();
                }
            }
        }

        Voxel.Destroy();
    }
}
