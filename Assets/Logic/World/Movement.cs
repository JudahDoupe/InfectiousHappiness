using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Assets.Logic;
using Assets.Logic.World;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public Voxel StartingVoxel;
    public float Speed = 0.25f;
    public bool IsStunned;
    public bool IsBeingCarried;

    private Voxel _lastVoxel;
    private bool _isFalling;

    void Start()
    {
        _lastVoxel = Map.GetVoxel(transform.position);
        StartingVoxel = _lastVoxel;
    }

    public bool MoveToVoxel(Voxel vox)
    {
        if (!MovePathClear(vox) || IsStunned) return false;

        StartCoroutine(ExecuteMove(vox));

        return true;
    }
    public bool JumpToVoxel(Voxel vox)
    {
        if (!JumpPathClear(vox) || IsStunned) return false;

        StartCoroutine(ExecuteJump(vox));

        return true;
    }
    public bool Fall()
    {
        if (IsBeingCarried) return false;

        _isFalling = true;
        StartCoroutine(ExecuteFall());

        return true;
    }
    public void Reset()
    {
        StartCoroutine(ExecuteMove(StartingVoxel));
        gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }

    private bool MovePathClear(Voxel vox) 
    {
        var direction = (vox.Position - transform.position).normalized;
        var distance = Mathf.RoundToInt(Vector3.Distance(transform.position, vox.Position));

        for (var i = 1; i <= distance; i++)
        {
            if (Map.GetVoxel(transform.position + direction * i).Block != null)
                return false;
        }
        return true;
    }
    private bool JumpPathClear(Voxel vox)
    {
        var currentVox = Map.GetVoxel(transform.position);
        var height = Vector3.Distance(Vector3.Scale(vox.Position, -Map.GravityDirection), Vector3.Scale(transform.position, -Map.GravityDirection));
        for (var i = 0; i <= height; i++)
        {
            var voxInPath = Map.GetVoxel(transform.position - Map.GravityDirection * i);
            if (voxInPath != currentVox && voxInPath.Block != null)
                return false;
        }

        var direction = (vox.Position - transform.position).normalized;
        var distance = Mathf.RoundToInt(Vector3.Distance(transform.position, vox.Position));
        for (var t = 0f; t <= distance; t += 0.5f)
        {
            var jumpHeight = -(t * t) + distance * t;
            var voxInPath = Map.GetVoxel(transform.position + (-Map.GravityDirection * (height + jumpHeight)) + direction * t);
            if (voxInPath != currentVox && voxInPath.Block != null)
                return false;
        }

        return true;
    }

    private IEnumerator ExecuteMove(Voxel vox)
    {
        BeginMovement();

        var start = transform.position;
        var direction = (vox.Position - transform.position).normalized;
        var distance = Mathf.RoundToInt(Vector3.Distance(transform.position, vox.Position));

        for (var t = 0f; t < distance; t += (Speed / 60f))
        {
            transform.position = start + direction * t;
            yield return new WaitForFixedUpdate();
        }

        EndMovement(vox);
    }
    private IEnumerator ExecuteJump(Voxel vox)
    {
        BeginMovement();

        var start = transform.position;
        var height = Vector3.Distance( Vector3.Scale(vox.Position, -Map.GravityDirection), Vector3.Scale(transform.position, -Map.GravityDirection));
        for (var t = 0f; t <= height; t += (Speed / 60f))
        {
            transform.position = start -Map.GravityDirection * t;
            yield return new WaitForFixedUpdate();
        }

        var direction = (vox.Position - transform.position).normalized;
        var distance = Mathf.RoundToInt(Vector3.Distance(transform.position, vox.Position));
        for (var t = 0f; t <= distance; t += (Speed / 1.8f / 60f))
        {
            var jumpHeight = -(t * t) + distance * t;
            transform.position = start + (-Map.GravityDirection * (height + jumpHeight)) + direction * t;
            yield return new WaitForFixedUpdate();
        }

        EndMovement(vox);
    }
    private IEnumerator ExecuteFall()
    {

        BeginMovement();

        var velocity = Vector3.zero;
        var potentialFloor = Map.GetVoxel(transform.position - transform.up);

        while (potentialFloor.Block == null && Map.IsInsideMap(transform.position))
        {
            velocity = velocity + Map.GravityVector;
            transform.Translate(velocity);
            potentialFloor = Map.GetVoxel(transform.position - transform.up);
            yield return new WaitForFixedUpdate();
        }

        if (potentialFloor.Block != null)
            EndMovement();
        else
            Reset();
    }

    private void Bounce()
    {
        var floor = Map.GetVoxel(transform.position + Map.GravityDirection);

        SoundFX.Instance.PlayClip(SoundFX.Instance.Bounce);
        var v = _lastVoxel.Position - floor.Position;
        var target = Map.GetVoxel(floor.Position - v + 2 * Vector3.Dot(v, -Map.GravityDirection) * -Map.GravityDirection);

        if(!JumpToVoxel(target))
            StartCoroutine(ExecuteJump(_lastVoxel));
    }

    private bool BeginMovement()
    {
        IsStunned = true;
        _lastVoxel.Block = null;
        return true;
    }
    private bool EndMovement(Voxel vox = null)
    {
        IsStunned = false;
        if (vox == null)
            vox = Map.GetVoxel(transform.position);

        var floor = Map.GetVoxel(vox.Position - transform.up);

        if (floor.Block == null)
        {
            if (!Fall())
                Reset();
            return false;
        }

        if (floor.Block.Type == BlockType.Bouncy)
        {
            Bounce();
            return false;
        }

        if (gameObject.GetComponent<Character>())
            floor.Block.Infect();

        if (_isFalling && transform.GetComponent<Block>())
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Drop);

        _isFalling = false;

        _lastVoxel = vox;
        _lastVoxel.Block = transform.GetComponent<Block>();
        transform.position = _lastVoxel.Position;

        return true;
    }
}
