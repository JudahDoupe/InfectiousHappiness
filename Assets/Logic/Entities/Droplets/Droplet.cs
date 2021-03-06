﻿
using System;
using System.Collections;
using UnityEngine;

public class Droplet : Entity, IMovable, IInteractable {

    public float MovementSpeed = 5;
    public Vector3? MovementVector
    {
        get
        {
            if (_lastStationaryPosition == null) return null;
            return transform.position - _lastStationaryPosition.Value;
        }
    }

    void Start()
    {
        Class = "Droplet";
        UpdateMaterial();
    }

    private Vector3? _lastStationaryPosition;
    private bool _isMoving;
    private bool _isFalling;
    private bool _isOnPath;

    public void Interact(Entity actor)
    {
        if (actor is Character)
            (actor as Character).Lift(this);
    }

    public bool IsMoving()
    {
        return _isMoving || _isFalling || _isOnPath;
    }
    public void Reset()
    {
        Destroy(gameObject);
    }
    public void Fall()
    {
        if (Voxel == null) return;
        StartCoroutine(_Fall());
    }
    public void MoveTo(Voxel vox, bool moveThroughBlocks = false, bool enterVoxel = true)
    {
        if (Voxel == null && !moveThroughBlocks) return;
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
        while (floorVox != null && floorVox.Entity == null)
        {
            transform.position = transform.position - VoxelWorld.MainCharacter.transform.up * Time.deltaTime * MovementSpeed;
            floorVox = VoxelWorld.GetVoxel(transform.position - VoxelWorld.MainCharacter.transform.up * 0.6f);
            yield return new WaitForFixedUpdate();
        }

        if(floorVox == null)
        {
            Destroy(gameObject);
            yield break;
        }
        else if (floorVox.Entity is Block)
        {
            (floorVox.Entity as Block).Collide(this);
        }
        else if (floorVox.Entity is Character && (floorVox.Entity as Character).Load == null)
        {
            (floorVox.Entity as Character).Load = this;
            transform.position = floorVox.Entity.transform.position + VoxelWorld.MainCharacter.transform.up;
            transform.parent = floorVox.Entity.transform;
            _lastStationaryPosition = null;
        }
        else
        {
            VoxelWorld.GetVoxel(transform.position).Fill(this);
            _lastStationaryPosition = null;
        }
        _isFalling = true;
    }
    private IEnumerator _ArchTo(Voxel vox, bool forceMove)
    {
        _isMoving = true;
        if (Voxel != null)
        {
            _lastStationaryPosition = Voxel.WorldPosition;
            Voxel.Release();
        }

        var start = transform.position;
        var end = vox.WorldPosition;
        var height = 3f;
        var t = 0f;
        while (t < 1 && (!(VoxelWorld.GetVoxel(transform.position).Entity is Block) || forceMove))
        {
            var verticalOffset = new Vector3(0, (0.25f - Mathf.Pow(t - 0.5f, 2)) * height, 0);
            verticalOffset.Scale(VoxelWorld.MainCharacter.transform.up);
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / 2) + verticalOffset;
            yield return new WaitForFixedUpdate();
        }

        if (VoxelWorld.GetVoxel(transform.position).Entity is Block)
            (VoxelWorld.GetVoxel(transform.position).Entity as Block).Collide(this);
        else
            StartCoroutine(_Fall());
        _isMoving = false;
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
