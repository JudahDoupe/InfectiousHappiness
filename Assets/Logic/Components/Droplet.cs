using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droplet : MonoBehaviour {

    public DropleteType Type;
    public int SplashRadius = 3;

    public void SetType(string typeName)
    {
        switch (typeName)
        {
            case "Ink":
                Type = DropleteType.Ink;
                break;
            case "Water":
                Type = DropleteType.Water;
                break;
            case "Fire":
                Type = DropleteType.Fire;
                break;
            default:
                Type = DropleteType.Ink;
                break;
        }
    }

    public void Splash()
    {
        var r = SplashRadius;
        for (int i = -r; i <= r; i++)
        {
            for (int j = -r; j <= r; j++)
            {
                for (int k = -r; k <= r; k++)
                {
                    var vox = VoxelWorld.GetVoxel(transform.position + Vector3.right * i + Vector3.up * j + Vector3.forward * k);
                    if (vox != null && vox.Block != null && Vector3.Distance(Vector3.zero, new Vector3(i, j, k)) < r) 
                        vox.Block.Activate();
                }
            }
        }

        Destroy(gameObject);
    }
}

public enum DropleteType
{
    Ink,
    Water,
    Fire
}