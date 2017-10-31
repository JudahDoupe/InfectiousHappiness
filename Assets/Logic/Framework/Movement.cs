﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Assets.Logic;
using Assets.Logic.Framework;
using Assets.Logic.World;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class Movement : MonoBehaviour
{
    public Voxel StartingVoxel;
    public float Speed = 10;
    public bool IsStunned;

    private bool _isFalling;
    private Voxel _lastVoxel;
    private Movement _parent;

    public void Start()
    {
        _lastVoxel = Map.CurrentLevel.GetVoxel(transform.position);
        StartingVoxel = _lastVoxel;
    }

    public bool MoveToVoxel(Voxel vox)
    {
        if (IsStunned) return false;

        var start = Map.CurrentLevel.GetVoxel(transform.position);

        var direction = (vox.Position - start.Position).normalized;
        var distance = Vector3.Distance(start.Position, vox.Position);

        if (!MovePathClear(direction, distance)) return false;
        Debug.Log("Moving");

        BeginMovement();
        StartCoroutine(ExecuteMove(direction, distance));

        return true;
    }
    public bool JumpToVoxel(Voxel vox)
    {
        if (IsStunned)
            return false;

        var start = Map.CurrentLevel.GetVoxel(transform.position);

        var startHeight = Vector3.Scale(start.Position, -Map.CurrentLevel.GravityDirection).magnitude;
        var endHeight = Vector3.Scale(vox.Position, -Map.CurrentLevel.GravityDirection).magnitude;
        var height = endHeight - startHeight < 0 ? 0 : endHeight - startHeight;

        var parabolaStart = Map.CurrentLevel.GetVoxel(start.Position - Map.CurrentLevel.GravityDirection * height);

        var direction = (vox.Position - parabolaStart.Position).normalized;
        var distance = Vector3.Distance(parabolaStart.Position, vox.Position);

        if (!JumpPathClear(direction, distance, height))
            return false;

        BeginMovement();
        StartCoroutine(ExecuteJump(direction, distance, height));

        return true;
    }
    public bool Fall()
    {
        if (_parent != null) return false;

        _isFalling = true;
        StartCoroutine(ExecuteFall());

        return true;
    }
    public void Reset()
    {
        var start = Map.CurrentLevel.GetVoxel(transform.position);

        var direction = (StartingVoxel.Position - start.Position).normalized;
        var distance = Vector3.Distance(start.Position, StartingVoxel.Position);

        StartCoroutine(ExecuteMove(direction, distance));
        gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }

    public void Parent(Movement parent)
    {
        _parent = parent;
        transform.parent = parent.transform;
    }
    public void UnParent()
    {
        _parent = null;
        transform.parent = null;
        IsStunned = false;
    }

    private bool MovePathClear(Vector3 direction, float distance) 
    {
        var start = Map.CurrentLevel.GetVoxel(transform.position);
        for (var i = 1; i <= distance; i++)
        {
            if (Map.CurrentLevel.GetVoxel(start.Position + direction * i).HasBlock())
                return false;
        }
        return true;
    }
    private IEnumerator ExecuteMove(Vector3 direction, float distance)
    {
        var start = transform.position;

        for (var t = 0f; t <= distance; t += (Speed / 60f))
        {
            transform.position = start + direction * t;
            yield return new WaitForFixedUpdate();
        }

        EndMovement();
    }
    private bool JumpPathClear(Vector3 direction, float distance, float height)
    {
        var start = Map.CurrentLevel.GetVoxel(transform.position);
        for (var i = 0; i <= height; i++)
        {
            var voxInPath = Map.CurrentLevel.GetVoxel(start.Position - Map.CurrentLevel.GravityDirection * i);
            if (voxInPath != start && voxInPath.HasBlock())
                return false;
        }

        for (var x = 0f; x <= distance; x += 0.5f)
        {
            var y = -(x * x) + distance * x;
            var voxInPath = Map.CurrentLevel.GetVoxel(start.Position + (-Map.CurrentLevel.GravityDirection * (y + height)) + direction * x);
            if (voxInPath != start && voxInPath.HasBlock())
                return false;
        }
        return true;
    }
    private IEnumerator ExecuteJump(Vector3 direction, float distance, float height)
    {
        var start = Map.CurrentLevel.GetVoxel(transform.position);

        for (var t = 0f; t <= height; t += (Speed / 60f))
        {
            transform.position = start.Position - Map.CurrentLevel.GravityDirection * t;
            yield return new WaitForFixedUpdate();
        }

        for (var x = 0f; x <= distance; x += (Speed / 1.8f / 60f))
        {
            var y = -(x * x) + distance * x;
            transform.position = start.Position + (-Map.CurrentLevel.GravityDirection * (y + height)) + direction * x;
            yield return new WaitForFixedUpdate();
        }

        EndMovement();
    }
    private IEnumerator ExecuteFall()
    {
        var velocity = Vector3.zero;
        var potentialFloor = Map.CurrentLevel.GetVoxel(transform.position + Map.CurrentLevel.GravityDirection);

        while (!potentialFloor.HasBlock() && Map.CurrentLevel.IsInsideMap(transform.position))
        {
            velocity = velocity + Map.CurrentLevel.GravityVector;
            transform.Translate(velocity);
            potentialFloor = Map.CurrentLevel.GetVoxel(transform.position + Map.CurrentLevel.GravityDirection);
            yield return new WaitForFixedUpdate();
        }

        if (potentialFloor.HasBlock())
            EndMovement();
        else
            Reset();
    }

    private void Bounce()
    {
        var floor = Map.CurrentLevel.GetVoxel(transform.position + Map.CurrentLevel.GravityDirection);

        SoundFX.Instance.PlayClip(SoundFX.Instance.Bounce);
        var v = _lastVoxel.Position - floor.Position;
        var target = Map.CurrentLevel.GetVoxel(floor.Position - v + 2 * Vector3.Dot(v, -Map.CurrentLevel.GravityDirection) * -Map.CurrentLevel.GravityDirection);

        if(!JumpToVoxel(target))
            JumpToVoxel(_lastVoxel);
    }

    private bool BeginMovement()
    {
        IsStunned = true;
        _lastVoxel.Empty();
        _lastVoxel = Map.CurrentLevel.GetVoxel(transform.position);
        
        return true;
    }
    private bool EndMovement(Voxel vox = null)
    {
        if (_parent != null) return false;

        if (vox == null) vox = Map.CurrentLevel.GetVoxel(transform.position);
        var floor = Map.CurrentLevel.GetVoxel(vox.Position + Map.CurrentLevel.GravityDirection);

        if (!floor.HasBlock())
        {
            if(!Fall())Reset();
            return false;
        }

        IsStunned = false;
        if (floor.GetBlock().Type == BlockType.Bouncy)
        {
            Bounce();
            return false;
        }

        if (gameObject.GetComponent<Character>())
            floor.GetBlock().Activate();

        if (_isFalling && transform.GetComponent<Block>())
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Drop);

        _isFalling = false;
        _lastVoxel = vox;
        _lastVoxel.Fill(gameObject);

        return true;
    }

}