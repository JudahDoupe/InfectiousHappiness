using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Assets.Logic;
using Assets.Logic.Framework;
using UnityEngine;


public class Character : MonoBehaviour
{
    public enum CharacterType
    {
        Player,
        Builder,
    }

    public CharacterType Type;
    public Movement Load;
    public Movement Movement;

    private Voxel _cursorPosition;
    private Renderer _cursor;

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
    }

    void Update()
    {
        if (Type == CharacterType.Player)
        {
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
        }
        else if (Type == CharacterType.Builder)
        {
            _cursorPosition = VoxelWorld.GetVoxel(transform.position + transform.forward);

            if (_cursorPosition != null)
            {
                _cursor.transform.position = _cursorPosition.Position;
                _cursor.enabled = true;
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
            else if (Input.GetKeyDown(KeyCode.S))
                transform.Rotate(new Vector3(0,0,180));
            else if (Input.GetButtonDown("Primary"))
                PlaceBlock();
            else if (Input.GetButtonDown("Secondary"))
                RemoveBlock();
            else if (Input.GetKeyDown(KeyCode.L))
                VoxelWorld.LoadLevel(VoxelWorld.ActiveLevel);
            else if (Input.GetKeyDown(KeyCode.U))
                VoxelWorld.UnLoadLevel(VoxelWorld.ActiveLevel);
        }

        if (_cursorPosition == null)
            _cursor.enabled = false;

    }

    // Input Commands
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
    public void Jump()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward * 2));
    }
    public void Climb()
    {
        Movement.JumpToVoxel(VoxelWorld.GetVoxel(transform.position + transform.forward - VoxelWorld.GravityVector.normalized));
    }
    public void Die()
    {
        if (Load != null)
            Load.Drop(this);
        SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
    }

    //Building
    public void PlaceBlock()
    {
        return;
    }
    public void RemoveBlock()
    {
        return;
    }

    // Queries
    public Block GetFloorBlock()
    {
        var vox = VoxelWorld.GetVoxel(transform.position - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardBlock()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardGap()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }
    public Block GetForwardGapFloor()
    {
        var vox = VoxelWorld.GetVoxel(transform.position + transform.forward - transform.up * 2);
        if (vox != null && vox.Block)
            return vox.Block;

        return null;
    }

}
