using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : Block {

    void Start()
    {
        Class = "Block";
        Type = "Bounce";
        UpdateMaterial();
    }

    public override void Collide(Droplet droplet)
    {
        var direction = droplet.MovementVector;
        Debug.Log(direction);
        if (!direction.HasValue)
        {
            Destroy(droplet.gameObject);
            return;
        }

        VoxelWorld.GetVoxel(transform.position + Vector3.up).Fill(droplet);
        var normal = (droplet.transform.position - transform.position).normalized;
        var r = direction.Value - 2 * Vector3.Dot(direction.Value, normal) * normal;
        var newPos = transform.position + r;
        var newVox = VoxelWorld.GetVoxel(newPos);
        droplet.MoveTo(newVox);
    }
}
