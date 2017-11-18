using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class Movement : MonoBehaviour
{
    public Voxel SpawnVoxel;
    public float Speed = 10;
    public bool IsStunned;

    private bool _isFalling;
    private Voxel _lastVoxel;
    private Movement _parent;

    public void Start()
    {
        _lastVoxel = World.GetVoxel(transform.position);
        SpawnVoxel = _lastVoxel;
    }

    // Commands
    public bool MoveToVoxel(Voxel vox)
    {
        if (IsStunned) return false;

        var start = World.GetVoxel(transform.position);

        var direction = (vox.Position - start.Position).normalized;
        var distance = Vector3.Distance(start.Position, vox.Position);

        if (!MovePathClear(direction, distance)) return false;

        BeginMovement();
        StartCoroutine(ExecuteMove(direction, distance));

        return true;
    }
    public bool JumpToVoxel(Voxel vox)
    {
        if (IsStunned)
            return false;

        var start = World.GetVoxel(transform.position);

        var startHeight = Vector3.Scale(start.Position, -World.GravityVector.normalized).magnitude;
        var endHeight = Vector3.Scale(vox.Position, -World.GravityVector.normalized).magnitude;
        var height = endHeight - startHeight < 0 ? 0 : endHeight - startHeight;

        var parabolaStart = World.GetVoxel(start.Position - World.GravityVector.normalized * height);

        var direction = (vox.Position - parabolaStart.Position).normalized;
        var distance = Vector3.Distance(parabolaStart.Position, vox.Position);

        if (!JumpPathClear(direction, distance, height))
            return false;

        BeginMovement();
        StartCoroutine(ExecuteJump(direction, distance, height));

        return true;
    }
    public bool Transport(Stack<Voxel> path)
    {
        BeginMovement();
        StartCoroutine(ExecuteTransport(new Stack<Voxel>(path)));
        return true;
    }

    public void Push(Character pusher)
    {
        if (IsStunned) return;

        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Push);
        StartCoroutine("MoveToVoxel", World.GetVoxel(transform.position + pusher.transform.forward));
    }
    public bool Lift(Character lifter)
    {
        if (IsStunned) return false;

        if (!JumpToVoxel(World.GetVoxel(lifter.transform.position + lifter.transform.up))) return false;

        lifter.Movement.IsStunned = true;
        lifter.Load = this;
        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
        Parent(lifter.Movement);

        return true;
    }
    public bool Drop(Character dropper)
    {
        if (IsStunned) return false;

        dropper.Load = null;
        UnParent();
        if (JumpToVoxel(World.GetVoxel(transform.position + dropper.transform.forward)))
            return true;

        Parent(dropper.Movement);
        MoveToVoxel(World.GetVoxel(transform.position));
        return false;
    }

    public bool Fall()
    {
        if(World.GetVoxel(transform.position + World.GravityVector.normalized) == null)
            return false;

        _isFalling = true;
        StartCoroutine(ExecuteFall());

        return true;
    }
    public void Bounce()
    {
        var floor = World.GetVoxel(transform.position + World.GravityVector.normalized);

        SoundFX.Instance.PlayClip(SoundFX.Instance.Bounce);
        var v = _lastVoxel.Position - floor.Position;
        var target = World.GetVoxel(floor.Position - v + 2 * Vector3.Dot(v, -World.GravityVector.normalized) * -World.GravityVector.normalized);

        if(!JumpToVoxel(target))
            JumpToVoxel(_lastVoxel);
    }
    public void Reset()
    {
        var direction = (SpawnVoxel.Position - transform.position).normalized;
        var distance = Vector3.Distance(transform.position, SpawnVoxel.Position);

        StartCoroutine(ExecuteMove(direction, distance));
        gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }

    // Utilities
    private bool MovePathClear(Vector3 direction, float distance) 
    {
        var start = World.GetVoxel(transform.position);
        for (var i = 1; i <= distance; i++)
        {
            if (World.GetVoxel(start.Position + direction * i).HasBlock())
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
        var start = World.GetVoxel(transform.position);
        for (var i = 0; i <= height; i++)
        {
            var voxInPath = World.GetVoxel(start.Position - World.GravityVector.normalized * i);
            if (voxInPath != start && voxInPath.HasBlock())
                return false;
        }

        for (var x = 0f; x <= distance; x += 0.5f)
        {
            var y = -(x * x) + distance * x;
            var voxInPath = World.GetVoxel(start.Position + (-World.GravityVector.normalized * (y + height)) + direction * x);
            if (voxInPath != start && voxInPath.HasBlock())
                return false;
        }
        return true;
    }
    private IEnumerator ExecuteJump(Vector3 direction, float distance, float height)
    {
        var start = World.GetVoxel(transform.position);
        for (var t = 0f; t <= height; t += (Speed / 60f))
        {
            transform.position = start.Position - World.GravityVector.normalized * t;
            yield return new WaitForFixedUpdate();
        }

        for (var x = 0f; x <= distance; x += (Speed / 1.8f / 60f))
        {
            var y = -(x * x) + distance * x;
            transform.position = start.Position + (-World.GravityVector.normalized * (y + height)) + direction * x;
            yield return new WaitForFixedUpdate();
        }

        EndMovement();
    }
    private IEnumerator ExecuteFall()
    {
        var velocity = Vector3.zero;
        var potentialFloor = World.GetVoxel(transform.position + World.GravityVector.normalized);

        while ((potentialFloor == null || potentialFloor.IsEmpty()) && World.IsInsideWorld(transform.position))
        {
            velocity = velocity + World.GravityVector;
            transform.Translate(velocity,Space.World);
            potentialFloor = World.GetVoxel(transform.position + World.GravityVector.normalized);
            yield return new WaitForFixedUpdate();
        }

        if (!World.IsInsideWorld(transform.position))
            Reset();
        else
            EndMovement();
    }
    private IEnumerator ExecuteTransport(Stack<Voxel> path)
    {
        while (path.Count > 0)
        {
            var start = transform.position;
            var newVox = path.Pop();
            var direction = (newVox.Position - start).normalized;
            var distance = Vector3.Distance(start, newVox.Position);

            for (var t = 0f; t <= distance; t += (Speed / 60f))
            {
                transform.position = start + direction * t;
                yield return new WaitForFixedUpdate();
            }
        }


        EndMovement();
    }

    private void Parent(Movement parent)
    {
        _parent = parent;
        transform.parent = parent.transform;
    }
    private void UnParent()
    {
        _parent = null;
        transform.parent = null;
        IsStunned = false;
    }

    private void BeginMovement()
    {
        IsStunned = true;
        _lastVoxel.Empty();
        _lastVoxel = World.GetVoxel(transform.position);
    }
    private void EndMovement(Voxel vox = null)
    {
        if (vox == null) vox = World.GetVoxel(transform.position);
        var floor = World.GetVoxel(vox.Position + World.GravityVector.normalized);

        if (floor != null && floor.HasCharacter() && _parent == null)
        {
            IsStunned = false;
            Lift(floor.GetCharacter());
            return;
        }

        if (floor == null || floor.IsEmpty())
        {
            if(!Fall())
                Reset();
            return;
        }

        /* Clear to stop moving */

        if (_isFalling && transform.GetComponent<Block>())
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Drop);

        IsStunned = false;
        _isFalling = false;

        if (_parent != null)
        {
            _parent.IsStunned = false;
            return;
        }
        if (!floor.GetBlock().Stand(this))
            return;

        /* Clear to land */

        _lastVoxel = vox;
        _lastVoxel.Fill(gameObject);
    }

}
