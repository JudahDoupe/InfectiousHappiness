using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Character : Entity, IMovable
{
    public bool IsBuilder;
    public GameObject CursorModel;
    public GameObject PlayerModel;

    [Header("Player Settings")]
    public float MovementSpeed = 5;

    private IMovable _load;
    public IMovable Load
    {
        set
        {
            _load = value;
            var floor = VoxelWorld.GetVoxel(transform.position - transform.up);
            if (floor != null) floor.Puzzle.UpdateActiveBlocks(_load);
        }
        get { return _load; }
    }

    [Header("Builder Settings")]
    public string EntityName = "";
    public string EntityType = "";
    public int PuzzleNumber;

    void Update()
    {
        PlayerModel.SetActive(!IsBuilder);
        CursorModel.SetActive(IsBuilder);
        BuilderControls();
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
        if (VoxelWorld.SpawnVoxel == null) return;

        if (Voxel != null) Voxel.Release();
        _isFalling = false;
        _isMoving = false;
        _isOnPath = false;
        VoxelWorld.SpawnVoxel.Fill(this);

        if (!IsBuilder)
            Fall();
    }
    public void Fall()
    {
        if (Voxel == null) return;
        _isFalling = true;
        StartCoroutine(_Fall());
    }
    public void MoveTo(Voxel vox, bool forceMove = false)
    {
        if (Voxel == null) return;
        _isMoving = true;
        if (Mathf.Abs(Voxel.WorldPosition.y - vox.WorldPosition.y) < 0.1f)
            StartCoroutine(_MoveTo(vox, forceMove));
        else
            StartCoroutine(_ArchTo(vox, forceMove));
    }
    public void FollowPath(Voxel[] path, bool forceMove = true)
    {
        if (Voxel == null) return;
        _isOnPath = true;
        StartCoroutine(_FollowPath(path, forceMove));
    }

    private IEnumerator _Fall()
    {
        if(Voxel != null)Voxel.Release();

        var floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        while (floorVox != null && !(floorVox.Entity is Block))
        {
            transform.position =  transform.position + (Vector3.down * Time.deltaTime * MovementSpeed);
            floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
            yield return new WaitForFixedUpdate();
            if(!_isFalling)yield break;
        }
        _isFalling = false;

        if (floorVox == null)
            Reset();
        else if (floorVox.Entity is Block)
        {
            VoxelWorld.GetVoxel(transform.position).Fill(this);
        }
    }
    private IEnumerator _MoveTo(Voxel vox, bool forceMove)
    {
        Voxel.Release();

        var start = transform.position;
        var end = vox.WorldPosition;
        var t = 0f;
        var forward  = (end - start).normalized;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        while (t < 1 && forwardVox != null && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed);
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
            yield return new WaitForFixedUpdate();
            if (!_isMoving) yield break;
        }

        _isMoving = false;
        VoxelWorld.GetVoxel(transform.position).Fill(this);
        Fall();
    }
    private IEnumerator _ArchTo(Voxel vox, bool forceMove)
    {
        Voxel.Release();

        var start = transform.position;
        var end = vox.WorldPosition;
        var height = 3.25f;
        var t = 0f;
        var forward = (end - start).normalized;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / 2) + new Vector3(0, (0.25f - Mathf.Pow(t-0.5f,2))* height, 0);
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
            yield return new WaitForFixedUpdate();
            if(!_isMoving)yield break;
        }

        _isMoving = false;
        VoxelWorld.GetVoxel(transform.position).Fill(this);
        Fall();
    }
    private IEnumerator _FollowPath(Voxel[] path, bool forceMove)
    {
        foreach (var voxel in path)
        {
            while (Voxel == null)
                yield return new WaitForEndOfFrame();
            if (!_isOnPath) yield break;
            MoveTo(voxel, forceMove);
        }
        _isOnPath = false;
    }

    public void Lift(IMovable entity)
    {
        StartCoroutine(_Lift(entity));
    }
    public void Throw(Voxel target)
    {
        var load = Load;
        Load = null;
        VoxelWorld.GetVoxel(transform.position + transform.up).Fill(load as Entity);
        load.MoveTo(target);
    }

    private IEnumerator _Lift(IMovable entity)
    {
        while (IsMoving())
            yield return new WaitForFixedUpdate();
        var e = (entity as Entity);
        if (Mathf.Abs(e.transform.position.y - transform.position.y) < 0.1f && 
            Vector3.Distance(transform.position, e.transform.position) < 1.1f)
            entity.MoveTo(VoxelWorld.GetVoxel(transform.position + transform.up));
    }

    private void BuilderControls()
    {
        if (IsBuilder == false)return;

        if (Input.GetButtonDown("Up"))
            transform.position += transform.forward;
        else if (Input.GetButtonDown("Down"))
            transform.position -= transform.forward;
        else if (Input.GetButtonDown("Left"))
            transform.Rotate(new Vector3(0, -90, 0));
        else if (Input.GetButtonDown("Right"))
            transform.Rotate(new Vector3(0, 90, 0));
        else if (Input.GetButtonDown("Primary"))
            VoxelWorld.GetVoxel(transform.position).Fill(EntityConstructor.NewEntity(EntityName, EntityType), PuzzleNumber);
        else if (Input.GetButtonDown("Secondary"))
            VoxelWorld.GetVoxel(transform.position).Destroy();
        else if (Input.GetKeyDown(KeyCode.Space))
            transform.position += transform.up;
        else if (Input.GetKeyDown(KeyCode.LeftShift))
            transform.position -= transform.up;
        else if (Input.GetKeyDown(KeyCode.F))
            transform.Rotate(new Vector3(0, 0, 180));
        else if (Input.GetKeyDown(KeyCode.S))
            VoxelWorld.ActiveLevel.SaveAll();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Time.timeScale = Time.timeScale <= 0.1f ? 1 : 0;
    }

}
