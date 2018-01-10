using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum CharacterType
    {
        Player,
        Builder,
    }

    public CharacterType Type;
    [HideInInspector]
    public Movement Load;
    [HideInInspector]
    public Movement Movement;

    public int BuildingRoom;
    public GameObject BuildingMaterial;

    private Voxel _cursorPosition;
    private Renderer _cursor;

    private float doubleTapSpeed = 0.2f;
    private float minSwipeDistance = 25f;
    private Vector2 touchOrigin = Vector2.zero;
    private bool tapping;

    void Start()
    {

        Movement = gameObject.GetComponent<Movement>() ?? gameObject.AddComponent<Movement>();
        Movement.SpawnVoxel = VoxelWorld.SpawnVoxel;
        transform.position = Movement.SpawnVoxel.Position;

        if(Type == CharacterType.Player)
            Movement.Fall();

        var cursorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cursor = cursorObj.GetComponent<Renderer>();
        _cursor.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        _cursor.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _cursor.material.SetInt("_ZWrite", 0);
        _cursor.material.DisableKeyword("_ALPHATEST_ON");
        _cursor.material.DisableKeyword("_ALPHABLEND_ON");
        _cursor.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        _cursor.material.renderQueue = 3000;
        _cursor.material.color = new Color(1, 1, 1, 0.25f);

        if (BuildingMaterial == null)
            BuildingMaterial = VoxelWorld.Instance.FloorBlock;
    }

    void Update()
    {
        if (Type == CharacterType.Player)
        {
            #if UNITY_STANDALONE || UNITY_WEBPLAYER

            _cursorPosition = null;

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
            else if (Input.GetKeyDown(KeyCode.Escape))
                Time.timeScale = Time.timeScale <= 0.001 ? 1 : 0;

            #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            
            if (Input.touchCount > 0)
            {
                Touch firstTouch = Input.touches[0];

                if (firstTouch.phase == TouchPhase.Began)
                {
                    touchOrigin = firstTouch.position;
                }
                else if (firstTouch.phase == TouchPhase.Ended)
                {
                    Vector2 touchDelta = firstTouch.position - touchOrigin;
                    if (touchDelta.magnitude >= minSwipeDistance) //swipe
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
                        if (tapping)
                        {
                            Secondary();
                            tapping = false;
                        }
                        else
                            StartCoroutine(SingleTap());
                    }
                }
            }
            #endif

        }
        else if (Type == CharacterType.Builder)
        {
            _cursorPosition = VoxelWorld.GetVoxel(transform.position);

            if (_cursorPosition != null)
            {
                _cursor.transform.position = _cursorPosition.Position;
                _cursor.enabled = true;
                gameObject.GetComponent<Renderer>().enabled = false;
            }

            if (Input.GetButtonDown("Up"))
                transform.position += transform.forward;
            else if (Input.GetButtonDown("Down"))
                transform.position -= transform.forward;
            else if (Input.GetButtonDown("Left"))
                Left();
            else if (Input.GetButtonDown("Right"))
                Right();
            else if (Input.GetKeyDown(KeyCode.Space))
                transform.position += transform.up;
            else if (Input.GetKeyDown(KeyCode.LeftShift))
                transform.position -= transform.up;
            else if (Input.GetButtonDown("Primary"))
                PlaceBlock();
            else if (Input.GetButtonDown("Secondary"))
                RemoveBlock();
            else if (Input.GetKeyDown(KeyCode.C))
                transform.Rotate(new Vector3(0,0,180));
            else if (Input.GetKeyDown(KeyCode.S))
                VoxelWorld.ActiveLevel.SaveLevel();
        }

        if (_cursorPosition == null)
        {
            _cursor.enabled = false;
            gameObject.GetComponent<Renderer>().enabled = true;
        }

    }

    // Input Commands
    public IEnumerator SingleTap()
    {
        tapping = true;
        yield return new WaitForSeconds(doubleTapSpeed);
        if(tapping)
            Primary();
        tapping = false;
    }
    public void Forward()
    {
        if (GetForwardBlock() != null)
            Climb();
        else if (GetForwardGap() == null && GetForwardGapFloor() == null)
            Jump();
        else
            Movement.MoveToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward));
    }
    public void Back()
    {
        Movement.MoveToVoxel(VoxelWorld.GetVoxel(transform.position - transform.forward));
    }
    public void Right()
    {
        transform.Rotate(new Vector3(0, 90, 0));
    }
    public void Left()
    {
        transform.Rotate(new Vector3(0, -90, 0));
    }
    public void Primary()
    {
        if (Movement.IsStunned) return;
        var f = GetForwardBlock();
        if (f == null) return;

        switch (f.Type)
        {
            case BlockType.Movable:
                f.GetComponent<Movement>().Push(this);
                break;
            case BlockType.Switch:
                f.Interact(this);
                break;
            case BlockType.Pipe:
                f.Interact(this);
                break;
            default:
                break;
        }
    }
    public void Secondary()
    {
        if (Movement.IsStunned) return;

        if (Load != null)
        {
            Load.Drop(this);
            return;
        }

        var f = GetForwardBlock();
        if (f == null) return;

        switch (f.Type)
        {
            case BlockType.Movable:
                f.GetComponent<Movement>().Lift(this);
                break;
            case BlockType.Switch:
                f.Interact(this);
                break;
            default:
                break;
        }
    }

    // Movement
    private void Jump()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward * 2));
    }
    private void Climb()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward - VoxelWorld.GravityVector.normalized));
    }
    private void Die()
    {
        if (Load != null)
            Load.Drop(this);
        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
    }

    //Building
    private void PlaceBlock()
    {
        var vox = VoxelWorld.GetVoxel(_cursor.transform.position);
        if (vox == null) return;
        var obj = Instantiate(BuildingMaterial, vox.Position, Quaternion.identity);
        vox.Fill(obj, BuildingRoom);
    }
    private void RemoveBlock()
    {
        var vox = VoxelWorld.GetVoxel(_cursor.transform.position);
        if (vox != null)
            vox.DestroyObject();
    }

    // Queries
    private Block GetFloorBlock()
    {
        var vox = VoxelWorld.GetVoxel(transform.position - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    private Block GetForwardBlock()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    private Block GetForwardGap()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    private Block GetForwardGapFloor()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up * 2);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }

}
