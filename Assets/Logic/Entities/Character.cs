using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity, IMovable {

    public enum CharacterType
    {
        Player,
        Builder,
    }
    public CharacterType Mode;
    public GameObject CursorModel;
    public GameObject PlayerModel;

    [Header("Player Settings")]
    public float MovementSpeed = 5;
    public IMovable Load;

    [Header("Builder Settings")]
    public string EntityName = "";
    public string EntityType = "";
    public int PuzzleNumber;

    void Start()
    {
        Type = Mode.ToString();
        PlayerModel = transform.Find("Model").gameObject;
        CursorModel = transform.Find("Cursor").gameObject;
    }
    void Update()
    {
        if (Type != Mode.ToString())
            UpdateType();
        
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
        VoxelWorld.SpawnVoxel.Fill(this);
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
            yield return new WaitForFixedUpdate();
            floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        }
        _isFalling = false;

        if (floorVox == null)
            Reset();
        else if (floorVox.Entity is Block)
        {
            VoxelWorld.GetVoxel(transform.position).Fill(this);
            (floorVox.Entity as Block).Dye();
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
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
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
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
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
        entity.MoveTo(VoxelWorld.GetVoxel(transform.position + transform.up));
    }

    private void BuilderControls()
    {
        if (Mode != CharacterType.Builder)return;

        if (Input.GetButtonDown("Up"))
            transform.position += transform.forward;
        else if (Input.GetButtonDown("Down"))
            transform.position -= transform.forward;
        else if (Input.GetButtonDown("Left"))
            transform.Rotate(new Vector3(0, 90, 0));
        else if (Input.GetButtonDown("Right"))
            transform.Rotate(new Vector3(0, -90, 0));
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
    private void UpdateType()
    {
        Type = Mode.ToString();
        switch (Mode)
        {
            case CharacterType.Player:
                PlayerModel.SetActive(true);
                CursorModel.SetActive(false);
                break;
            case CharacterType.Builder:
                PlayerModel.SetActive(false);
                CursorModel.SetActive(true);
                break;
        }
    }
}
