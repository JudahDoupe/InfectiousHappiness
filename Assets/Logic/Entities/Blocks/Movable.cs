using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Movable : Block, IMovable
{
    public float MovementSpeed = 5;

    private Voxel _spawn;

    void Start()
    {
        Class = "Block";
        Type = "Movable";
        UpdateMaterial();
        StartCoroutine(SetSpawn());
    }
    private IEnumerator SetSpawn()
    {
        while (Voxel == null)
            yield return new WaitForFixedUpdate();
        _spawn = Voxel;
    }

    //Movement Methods
    private bool _isMoving;
    private bool _isFalling;
    private bool _isOnPath;

    public bool IsMoving()
    {
        return _isMoving || _isFalling || _isOnPath;
    }
    public void Reset()
    {
        StartCoroutine(_MoveTo(_spawn, true));
    }
    public void Fall()
    {
        if (Voxel == null) return;
        StartCoroutine(_Fall());
    }
    public void MoveTo(Voxel vox, bool moveThroughBlocks = false, bool enterVoxel = true)
    {
        if (Voxel == null) return;
        if (Mathf.Abs(Voxel.WorldPosition.y - vox.WorldPosition.y) < 0.1f)
            StartCoroutine(_MoveTo(vox, moveThroughBlocks));
        else
            StartCoroutine(_ArchTo(vox, moveThroughBlocks));
    }
    public void FollowPath(Voxel[] path, bool moveThroughBlocks = true)
    {
        if (Voxel == null) return;
        StartCoroutine(_FollowPath(path, moveThroughBlocks));
    }

    private IEnumerator _Fall()
    {
        _isFalling = true;
        if (Voxel != null) Voxel.Release();

        var floorVox = VoxelWorld.GetVoxel(transform.position - VoxelWorld.MainCharacter.transform.up * 0.6f);
        while (floorVox != null && !(floorVox.Entity is Block) && !(floorVox.Entity is Character))
        {
            transform.position = transform.position - VoxelWorld.MainCharacter.transform.up * Time.deltaTime * MovementSpeed;
            yield return new WaitForFixedUpdate();
            floorVox = VoxelWorld.GetVoxel(transform.position - VoxelWorld.MainCharacter.transform.up * 0.6f);
        }

        if (floorVox == null)
        {
            Reset();
        }
        else if (floorVox.Entity is Character && (floorVox.Entity as Character).Load == null)
        {
            (floorVox.Entity as Character).Load = this;
            transform.position = floorVox.Entity.transform.position + VoxelWorld.MainCharacter.transform.up;
            transform.parent = floorVox.Entity.transform;
        }
        else
        {
            VoxelWorld.GetVoxel(transform.position).Fill(this);
        }
        _isFalling = false;
    }
    private IEnumerator _MoveTo(Voxel vox, bool forceMove)
    {
        _isMoving = true;
        if (Voxel != null) Voxel.Release();

        var start = transform.position;
        var end = vox.WorldPosition;
        var forward = (end - start).normalized;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        var t = 0f;
        var d = Vector3.Distance(start, end);
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / d);
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        }

        _isMoving = false;
        StartCoroutine(_Fall());
    }
    private IEnumerator _ArchTo(Voxel vox, bool forceMove)
    {
        _isMoving = true;
        if (Voxel != null) Voxel.Release();
        var forward = (vox.WorldPosition - transform.position).normalized;

        var start = transform.position;
        var end = vox.WorldPosition;
        var height = 3f;
        var t = 0f;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        var d = Vector3.Distance(start, end);
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / (d*1.2f)) + new Vector3(0, (0.25f - Mathf.Pow(t - 0.5f, 2)) * height, 0);
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        }

        _isMoving = false;
        StartCoroutine(_Fall());
    }
    private IEnumerator _FollowPath(Voxel[] path, bool forceMove)
    {
        _isOnPath = true;
        foreach (var voxel in path)
        {
            while (Voxel == null)
                yield return new WaitForEndOfFrame();
            MoveTo(voxel, forceMove);
        }
        _isOnPath = false;
    }
}
