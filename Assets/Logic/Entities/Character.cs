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
    private GameObject _cursorModel;
    private GameObject _playerModel;

    [Header("Player Settings")]
    public float MovementSpeed = 5;
    public IMovable Load;

    [Header("Builder Settings")]
    public string EntityName = "";
    public string EntityType = "";
    public int PuzzleNumber;

    //Control Properties
    private float _doubleTapSpeed = 0.2f;
    private const float MinSwipeDistance = 25f;
    private Vector2 _touchOrigin;
    private bool _isTapping;

    void Start()
    {
        _playerModel = transform.Find("Model").gameObject;
        _cursorModel = transform.Find("Cursor").gameObject;

        VoxelWorld.SpawnVoxel.Fill(this);
        if (Mode == CharacterType.Player)
            Reset();
    }
    void Update()
    {
        Type = Mode.ToString();
        switch (Mode)
        {
            case CharacterType.Player:
                _playerModel.SetActive(true);
                _cursorModel.SetActive(false);
                break;
            case CharacterType.Builder:
                _playerModel.SetActive(false);
                _cursorModel.SetActive(true);
                break;
        }
        #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        ListenForKeyboardInput();
        #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        ListenForMobileInput();
        #endif
    }

    //Movement Methods
    public void Reset()
    {
        VoxelWorld.SpawnVoxel.Fill(this);
        Fall();
    }
    public void Fall()
    {
        if (Voxel == null) return;
        StartCoroutine(_Fall());
    }
    public void MoveTo(Voxel vox, bool forceMove = false)
    {
        if (Voxel == null) return;
        StartCoroutine(_MoveTo(vox,forceMove));
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
        if(Voxel != null)Voxel.Release();

        var floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        while (floorVox != null && !(floorVox.Entity is Block))
        {
            transform.position =  transform.position + (Vector3.down * Time.deltaTime * MovementSpeed);
            yield return new WaitForFixedUpdate();
            floorVox = VoxelWorld.GetVoxel(transform.position + Vector3.down * 0.6f);
        }

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
        if (Voxel != null) Voxel.Release();
        var forward  = (vox.WorldPosition - transform.position).normalized;

        var start = transform.position;
        var end = vox.WorldPosition;
        var t = 0f;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        while (t < 1 && forwardVox != null && (!(forwardVox.Entity is Block) || forceMove))
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
        var height = 3.25f;
        var t = 0f;
        var forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        while (t < 1 && (!(forwardVox.Entity is Block) || forceMove))
        {
            transform.position = Vector3.Lerp(start, end, t += Time.deltaTime * MovementSpeed / 2) + new Vector3(0, (0.25f - Mathf.Pow(t-0.5f,2))* height, 0);
            yield return new WaitForFixedUpdate();
            forwardVox = VoxelWorld.GetVoxel(transform.position + forward * 0.6f);
        }

        StartCoroutine(_Fall());
    }

    //Control Methods
    private void ListenForKeyboardInput()
    {
        if (Input.GetButtonDown("Up"))
            Forward();
        else if (Input.GetButtonDown("Down"))
            Back();
        else if (Input.GetButtonDown("Left"))
            Left();
        else if (Input.GetButtonDown("Right"))
            Right();
        else if (Input.GetButtonDown("Primary"))
            Primary();
        else if (Input.GetButtonDown("Secondary"))
            Secondary();
        else if (Input.GetKeyDown(KeyCode.Space) && Mode == CharacterType.Builder)
            transform.position += transform.up;
        else if (Input.GetKeyDown(KeyCode.LeftShift) && Mode == CharacterType.Builder)
            transform.position -= transform.up;
        else if (Input.GetKeyDown(KeyCode.F) && Mode == CharacterType.Builder)
            transform.Rotate(new Vector3(0, 0, 180));
        else if (Input.GetKeyDown(KeyCode.S))
            VoxelWorld.ActiveLevel.SaveAll();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Time.timeScale = Time.timeScale <= 0.1f ? 1 : 0;
    }
    private void ListenForMobileInput()
    {
        if (Input.touchCount <= 0) return;
        var firstTouch = Input.touches[0];

        switch (firstTouch.phase)
        {
            case TouchPhase.Began:
                _touchOrigin = firstTouch.position;
                break;
            case TouchPhase.Ended:
                var touchDelta = firstTouch.position - _touchOrigin;
                if (touchDelta.magnitude >= MinSwipeDistance) //swipe
                {
                    if (Mathf.Abs(touchDelta.y) > Mathf.Abs(touchDelta.x))
                        if (touchDelta.y >= 0)
                            Forward();
                        else
                            Back();
                    else if (touchDelta.x >= 0)
                        Right();
                    else
                        Left();

                }
                else //tap
                {
                    if (_isTapping)
                    {
                        Secondary();
                        _isTapping = false;
                    }
                    else
                        StartCoroutine(SingleTap());
                }
                break;
        }
    }
    private IEnumerator SingleTap()
    {
        _isTapping = true;
        yield return new WaitForSeconds(_doubleTapSpeed);
        if (_isTapping)
            Primary();
        _isTapping = false;
    }

    private void Forward()
    {
        if (Mode == CharacterType.Builder)
        {
            transform.position += transform.forward;
            return;
        }

        if (!(VoxelWorld.GetVoxel(transform.position + transform.forward).Entity is Block))
            MoveTo(VoxelWorld.GetVoxel(transform.position + transform.forward));
        else if(!(VoxelWorld.GetVoxel(transform.position + transform.forward + transform.up).Entity is Block))
            ArchTo(VoxelWorld.GetVoxel(transform.position + transform.forward + transform.up));
    }
    private void Back()
    {
        if (Mode == CharacterType.Builder)
        {
            transform.position -= transform.forward;
            return;
        }

        MoveTo(VoxelWorld.GetVoxel(transform.position - transform.forward));
    }
    private void Right()
    {
        transform.Rotate(new Vector3(0, 90, 0));
    }
    private void Left()
    {
        transform.Rotate(new Vector3(0, -90, 0));
    }
    private void Primary()
    {
        if (Mode == CharacterType.Builder)
        {
            VoxelWorld.GetVoxel(transform.position).Fill(EntityConstructor.NewEntity(EntityName,EntityType),PuzzleNumber);
            return;
        }

        var forwardVox = VoxelWorld.GetVoxel(transform.position + transform.forward);
        if (forwardVox.Entity == null)
            return;
        else if (forwardVox.Entity is IInteractable)
            (forwardVox.Entity as IInteractable).InteractPrimary(this);
        else if (forwardVox.Entity is IMovable)
            (forwardVox.Entity as IMovable).MoveTo(VoxelWorld.GetVoxel(forwardVox.WorldPosition + transform.forward));
    }
    private void Secondary()
    {
        if (Mode == CharacterType.Builder)
        {
            VoxelWorld.GetVoxel(transform.position).Destroy();
            return;
        }

        if (Load != null)
        {
            Throw();
            return;
        }

        var forwardVox = VoxelWorld.GetVoxel(transform.position + transform.forward);
        if (forwardVox.Entity == null)
            return;
        else if (forwardVox.Entity is IInteractable)
            (forwardVox.Entity as IInteractable).InteractSecondary(this);
        else if (forwardVox.Entity is IMovable)
            Lift(forwardVox.Entity as IMovable);
    }

    private void Lift(IMovable entity)
    {
        entity.ArchTo(VoxelWorld.GetVoxel(transform.position + transform.up));
    }
    private void Throw()
    {
        var load = Load;
        Load = null;
        VoxelWorld.GetVoxel(transform.position + transform.up).Fill(load as Entity);
        if(load is Block)
            load.MoveTo(VoxelWorld.GetVoxel(transform.position + transform.up + transform.forward));
        else if (load is Droplet)
            load.ArchTo(VoxelWorld.GetVoxel(transform.position + transform.up + transform.forward * 5));
        else
            load.ArchTo(VoxelWorld.GetVoxel(transform.position + transform.up + transform.forward));
    }
}
