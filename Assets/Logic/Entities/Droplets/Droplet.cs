using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Droplet : Entity, IMovable {

    public int SplashRadius = 3;
    public float MovementSpeed = 5;

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
        if (Voxel == null) return;
        StartCoroutine(_Fall());
    }
    public void MoveTo(Voxel vox, bool forceMove = false)
    {
        if (Voxel == null) return;
        StartCoroutine(_MoveTo(vox, forceMove));
    }
    public void ArchTo(Voxel vox, bool forceMove = false)
    {
        if (Voxel == null) return;
        StartCoroutine(_ArchTo(vox, forceMove));
    }
    public void MoveAlongPath(Voxel[] path, bool forceMove = true)
    {
        if (Voxel == null) return;
        StartCoroutine(_MoveAlongPath(path, forceMove));
    }

    private IEnumerator _Fall()
    {
        if (Voxel != null) Voxel.Release();

        var floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        while (floorVox != null && !(floorVox.Entity is Block) && !(floorVox.Entity is Character))
        {
            transform.position = transform.position + (Vector3.down * Time.deltaTime * MovementSpeed);
            yield return new WaitForFixedUpdate();
            floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        }

        if (floorVox == null)
            Reset();
        else if (floorVox.Entity is Block)
            Splash();
        else if (floorVox.Entity is Character)
        {
            (floorVox.Entity as Character).Load = this;
            transform.position = floorVox.Entity.transform.position + Vector3.up;
            transform.parent = floorVox.Entity.transform;
        }
    }
    private IEnumerator _MoveTo(Voxel vox, bool forceMove)
    {
        if (Voxel != null) Voxel.Release();

        var start = transform.position;
        var end = vox.WorldPosition;
        var forward = (end - start).normalized;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        var t = 0f;
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed);
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        }
        if (forwardVox.Entity is Block)
            Splash();
        else
            StartCoroutine(_Fall());
    }
    private IEnumerator _MoveAlongPath(Voxel[] path, bool forceMove)
    {
        if (Voxel != null) Voxel.Release();

        foreach (var vox in path)
        {
            var start = transform.position;
            var end = vox.WorldPosition;
            var forward = (end - start).normalized;
            var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
            var t = 0f;
            while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
            {
                transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed);
                yield return new WaitForFixedUpdate();
                forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
            }
        }

        StartCoroutine(_Fall());
    }
    private IEnumerator _ArchTo(Voxel vox, bool forceMove)
    {
        if (Voxel != null) Voxel.Release();
        var forward = (vox.WorldPosition - transform.position).normalized;

        var start = transform.position;
        var end = vox.WorldPosition;
        var height = 3f;
        var t = 0f;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / 2) + new Vector3(0, (0.25f - Mathf.Pow(t - 0.5f, 2)) * height, 0);
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        }

        if (forwardVox.Entity is Block)
            Splash();
        else
            StartCoroutine(_Fall());
    }
}
