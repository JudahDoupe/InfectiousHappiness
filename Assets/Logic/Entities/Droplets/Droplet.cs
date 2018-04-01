
using System.Collections;
using UnityEngine;

public class Droplet : Entity, IMovable {

    public int SplashRadius = 3;
    public float MovementSpeed = 5;

    void Start()
    {
        Class = "Droplet";
        IsDyed = true;
        UpdateMaterial();
    }

    public virtual void Splash()
    {
        VoxelWorld.GetVoxel(transform.position).Fill(this);

        Voxel.Destroy();
    }

    private bool _isMoving;
    private bool _isFalling;
    private bool _isOnPath;

    public bool IsMoving()
    {
        return _isMoving || _isFalling || _isOnPath;
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
        StartCoroutine(_ArchTo(vox, forceMove));
    }
    public void FollowPath(Voxel[] path, bool forceMove = true)
    {
        if (Voxel == null) return;
        StartCoroutine(_FollowPath(path, forceMove));
    }

    private IEnumerator _Fall()
    {
        _isFalling = true;
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
            Splash();
        }
        _isFalling = true;
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
