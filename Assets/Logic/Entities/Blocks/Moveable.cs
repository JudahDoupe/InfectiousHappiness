using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class Moveable : Block, IMovable
{
    public float MovementSpeed = 5;

    private Voxel _spawn;

    void Start()
    {
        Class = "Block";
        Type = "Moveable";
        StartCoroutine(SetSpawn());
    }
    private IEnumerator SetSpawn()
    {
        while (Voxel == null)
            yield return new WaitForFixedUpdate();
        _spawn = Voxel;
    }

    //Movement Methods
    public void Reset()
    {
        MoveTo(_spawn, true);
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
        {
            Reset();
        }
        else if (floorVox.Entity is Character && (floorVox.Entity as Character).Load == null)
        {
            (floorVox.Entity as Character).Load = this;
            transform.position = floorVox.Entity.transform.position + Vector3.up;
            transform.parent = floorVox.Entity.transform;
        }
        else
        {
            VoxelWorld.GetVoxel(transform.position).Fill(this);
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

        StartCoroutine(_Fall());
    }
}
