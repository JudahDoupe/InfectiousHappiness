using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dispenser : Block
{

    private Voxel _output = null;

    void Start()
    {
        Class = "Block";
        Type = "Dispenser";
        UpdateMaterial();
        StartCoroutine(SetOutput());
    }

    void Update()
    {
        if (_output != null && _output.Entity == null)
        {
            var nextDroplet = VoxelWorld.GetVoxel(transform.position + Vector3.up).Entity;
            if (nextDroplet != null && nextDroplet is Droplet)
            {
                (nextDroplet as Droplet).Fall();
            }
        }
    }

    public override void Collide(Droplet droplet)
    {
        if (_output == null || _output.Entity != null)
        {
            VoxelWorld.GetVoxel(transform.position + Vector3.up).Fill(droplet, Voxel.Puzzle.Number);
        }
        else
        {
            _output.Fill(droplet, Voxel.Puzzle.Number);
            StartCoroutine(UpdateStack());
            
        }
    }

    private IEnumerator SetOutput()
    {
        var candidate = VoxelWorld.GetVoxel(transform.position + Vector3.down * 2);
        while (_output == null)
        {
            if (candidate.Entity != null && candidate.Entity is Droplet)
                _output = candidate;
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator UpdateStack()
    {
        for (int i = 2; i < 10; i++)
        {
            yield return new WaitForSeconds(0.3f);
            var nextDroplet = VoxelWorld.GetVoxel(transform.position + Vector3.up * i).Entity;
            if (nextDroplet == null || !(nextDroplet is Droplet)) yield break;
            (nextDroplet as Droplet).Fall();
        }
    }
}
