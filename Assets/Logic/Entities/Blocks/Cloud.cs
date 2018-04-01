using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : Block
{
    public string DropletType = "";

    private int _rainInterval;
    private int _rainIntervalMim = 60 * 5;
    private int _rainIntervalMax = 60 * 15;
    private int _rainTimer = 0;
    private Voxel _rainVox;

    void Start()
    {
        Class = "Block";
        Type = "Cloud";
        UpdateMaterial();
        _rainVox = VoxelWorld.GetVoxel(transform.position + Vector3.down);
        _rainInterval = Random.Range(_rainIntervalMim, _rainIntervalMax);
    }

    void FixedUpdate()
    {
        if (IsDyed && _rainTimer == 0 && _rainVox.Entity == null && DropletType != "")
        {
            var rain = EntityConstructor.NewDroplet(DropletType);
            _rainVox.Fill(rain);
            rain.Fall();
            _rainInterval = Random.Range(_rainIntervalMim, _rainIntervalMax);
        }

        _rainTimer = (_rainTimer + 1) % (_rainInterval);
    }

    public IEnumerator SpreadDye(string dropletType)
    {
        if(dropletType == DropletType) yield break;

        DropletType = dropletType;
        IsDyed = true;

        yield return new WaitForSeconds(Random.Range(0.2f,0.5f));

        var neighbors = VoxelWorld.GetNeighboringVoxels(Voxel.WorldPosition);
        foreach (var neighbor in neighbors)
        {
            if (!(neighbor.Entity is Cloud)) continue;
            var cloud = neighbor.Entity as Cloud;
            if (cloud.DropletType != dropletType) cloud.StartCoroutine(cloud.SpreadDye(dropletType));
        }
    }
}
