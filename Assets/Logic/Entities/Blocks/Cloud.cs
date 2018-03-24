using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : Block
{
    public string DroppletType = "";

    private int _rainInterval = 3;
    private int _rainTimer = 0;
    private Voxel _rainVox;

    void Start()
    {
        Class = "Block";
        Type = "Cloud";
        _rainVox = VoxelWorld.GetVoxel(transform.position + Vector3.down);
        _rainInterval = Random.Range(60, 250);
    }

    void FixedUpdate()
    {
        if (IsDyed && _rainTimer == 0 && _rainVox.Entity == null && DroppletType != "")
        {
            var rain = EntityConstructor.NewDroplet(DroppletType);
            _rainVox.Fill(rain);
            rain.Fall();
            _rainInterval = Random.Range(60, 250);
        }

        _rainTimer = (_rainTimer + 1) % (_rainInterval);
    }
}
