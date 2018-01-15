using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Voxel SpawnVoxel;
    public float Speed = 10;
    [HideInInspector]
    public bool IsStunned;
    [HideInInspector]
    public bool IsFalling;

    private Voxel _lastVoxel;
    private Movement _parent;

    public void Start()
    {
        _lastVoxel = VoxelWorld.GetVoxel(transform.position);
        SpawnVoxel = _lastVoxel;
    }

    // Commands
    public bool MoveToVoxel(Voxel vox)
    {
        if (IsStunned) return false;

        var start = VoxelWorld.GetVoxel(transform.position);

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

        var start = VoxelWorld.GetVoxel(transform.position);

        var startHeight = Vector3.Scale(start.Position, -VoxelWorld.GravityVector.normalized).magnitude;
        var endHeight = Vector3.Scale(vox.Position, -VoxelWorld.GravityVector.normalized).magnitude;
        var height = endHeight - startHeight < 0 ? 0 : endHeight - startHeight;

        var parabolaStart = VoxelWorld.GetVoxel(start.Position - VoxelWorld.GravityVector.normalized * height);

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
        StartCoroutine("MoveToVoxel", VoxelWorld.GetVoxel(transform.position + pusher.transform.forward));
    }
    public bool Lift(Character lifter)
    {
        if (IsStunned) return false;

        return JumpToVoxel(VoxelWorld.GetVoxel(lifter.transform.position + lifter.transform.up));
    }
    public bool Drop(Character dropper)
    {
        if (IsStunned) return false;

        dropper.Load = null;
        UnParent();
        if (JumpToVoxel(VoxelWorld.GetVoxel(transform.position + dropper.transform.forward)))
            return true;

        Parent(dropper.Movement);
        MoveToVoxel(VoxelWorld.GetVoxel(transform.position));
        return false;
    }

    public bool Fall()
    {
        if(VoxelWorld.GetVoxel(transform.position + VoxelWorld.GravityVector.normalized) == null)
            return false;

        IsFalling = true;
        StartCoroutine(ExecuteFall());

        return true;
    }
    public void Bounce()
    {
        var floor = VoxelWorld.GetVoxel(transform.position + VoxelWorld.GravityVector.normalized);

        SoundFX.Instance.PlayClip(SoundFX.Instance.Bounce);
        var v = _lastVoxel.Position - floor.Position;
        var target = VoxelWorld.GetVoxel(floor.Position - v + 2 * Vector3.Dot(v, -VoxelWorld.GravityVector.normalized) * -VoxelWorld.GravityVector.normalized);

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
        var start = VoxelWorld.GetVoxel(transform.position);
        for (var i = 1; i <= distance; i++)
        {
            if (VoxelWorld.GetVoxel(start.Position + direction * i).Block)
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
        var start = VoxelWorld.GetVoxel(transform.position);
        for (var i = 0; i <= height; i++)
        {
            var voxInPath = VoxelWorld.GetVoxel(start.Position - VoxelWorld.GravityVector.normalized * i);
            if (voxInPath != start && voxInPath.Block)
                return false;
        }

        for (var x = 0f; x <= distance; x += 0.5f)
        {
            var y = -(x * x) + distance * x;
            var voxInPath = VoxelWorld.GetVoxel(start.Position + (-VoxelWorld.GravityVector.normalized * (y + height)) + direction * x);
            if (voxInPath != start && voxInPath.Block)
                return false;
        }
        return true;
    }
    private IEnumerator ExecuteJump(Vector3 direction, float distance, float height)
    {
        var start = VoxelWorld.GetVoxel(transform.position);
        for (var t = 0f; t <= height; t += (Speed / 60f))
        {
            transform.position = start.Position - VoxelWorld.GravityVector.normalized * t;
            yield return new WaitForFixedUpdate();
        }

        for (var x = 0f; x <= distance; x += (Speed / 1.8f / 60f))
        {
            var y = -(x * x) + distance * x;
            transform.position = start.Position + (-VoxelWorld.GravityVector.normalized * (y + height)) + direction * x;
            yield return new WaitForFixedUpdate();
        }

        EndMovement();
    }
    private IEnumerator ExecuteFall()
    {
        var velocity = new Vector3(0,0,0);
        var potentialFloor = VoxelWorld.GetVoxel(transform.position + VoxelWorld.GravityVector.normalized);

        while (VoxelWorld.IsInsideWorld(transform.position + VoxelWorld.GravityVector.normalized) && potentialFloor.Block == null)
        {
            velocity = velocity + VoxelWorld.GravityVector;
            transform.Translate(velocity,Space.World);
            potentialFloor = VoxelWorld.GetVoxel(transform.position + VoxelWorld.GravityVector.normalized);
            yield return new WaitForFixedUpdate();
        }

        if (!VoxelWorld.IsInsideWorld(transform.position))
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
        _lastVoxel.TransferObject();
        _lastVoxel = VoxelWorld.GetVoxel(transform.position);
    }
    private void EndMovement(Voxel vox = null)
    {
        if (vox == null) vox = VoxelWorld.GetVoxel(transform.position);
        var floor = VoxelWorld.GetVoxel(vox.Position + VoxelWorld.GravityVector.normalized);

        if (floor != null && floor.Character != null && _parent == null) // We landed on a character
        {
            floor.Character.Load = this;
            Parent(floor.Character.Movement);
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            IsStunned = false;

            return;
        }

        if (floor == null || floor.Block == null) // There's nothing below us
        {
            if(!Fall())
                Reset();
            return;
        }

        /* Clear to stop moving */

        if (IsFalling && transform.GetComponent<Block>())
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Drop);

        IsStunned = false;
        IsFalling = false;

        if (_parent != null)
        {
            _parent.IsStunned = false;
            return;
        }
        if (!floor.Block.Stand(this))
            return;

        /* Clear to land */

        _lastVoxel = vox;
        _lastVoxel.Fill(gameObject);
    }

}
