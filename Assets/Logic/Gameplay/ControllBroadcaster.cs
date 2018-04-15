using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControllBroadcaster : MonoBehaviour
{
    public Text DebugOutput;
    public LayerMask LayerMask;
    public float TouchMovementTolerance = 50;

    private Vector2? _touchOrigin;
    private Vector2? _touchPos;

    public void Update()
    {
        #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
        UpdateClick();
        #elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        UpdateTouch();
        #endif
    }
    public void UpdateClick()
    {
        if (Input.GetMouseButtonDown(0))
            StartTouch(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            EndTouch(Input.mousePosition);
        else if (Input.GetMouseButton(0))
            MoveTouch(Input.mousePosition);
    }
    public void UpdateTouch()
    {
        if(Input.touches.Length == 0) return;
        var touch = Input.touches.First();
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartTouch(touch.position);
                break;
            case TouchPhase.Moved:
                MoveTouch(touch.position);
                break;
            case TouchPhase.Stationary:
                break;
            case TouchPhase.Ended:
                EndTouch(touch.position);
                break;
            case TouchPhase.Canceled:
                EndTouch(touch.position);
                break;
            default:
                break;
        }
    }

    public void StartTouch(Vector2 screenPos)
    {
        _touchOrigin = screenPos;
        _touchPos = screenPos;
    }
    public void MoveTouch(Vector2 screenPos)
    {
        if (_touchPos.HasValue == false) return;

        var delta = new Vector2(_touchOrigin.Value.y > Screen.height/2f ? -(screenPos.x - _touchPos.Value.x) : screenPos.x - _touchPos.Value.x, screenPos.y - _touchPos.Value.y);


        VoxelWorld.MainCamera.Move(delta / Screen.dpi * 0.75f);

        _touchPos = screenPos;
    }
    public void EndTouch(Vector2 screenPos)
    {
        _touchPos = null;
        if (Vector2.Distance(screenPos, _touchOrigin.Value) > TouchMovementTolerance) return;
 
        var vox = GetVoxel(screenPos);
        var player = VoxelWorld.MainCharacter;

        if (player == null || vox == null || vox.Entity == null) return;

        if (player.Load != null && vox.Entity.IsActive)
        {
            player.Throw(vox);
            return;
        }
        else if(player.Load == null && vox.Entity.IsActive)
        {
            player.FollowPath(Pathfinder.FindPath(player.Voxel, GetNearestWalkableVoxel(vox, player.Voxel)).ToArray());
            player.Lift(vox.Entity as IMovable);
            return;
        }
        else
        {
            var topVox = VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.up);
            player.FollowPath(Pathfinder.FindPath(player.Voxel, topVox).ToArray());
            return;
        }
    }

    public Voxel GetVoxel(Vector2 screenPos)
    {
        var ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            var voxel = VoxelWorld.GetVoxel(hit.transform.position);
            return voxel;
        }
        return null;
    }
    public Voxel GetNearestWalkableVoxel(Voxel target, Voxel start)
    {
        if (target == null || start == null) return null;
        var voxels = new List<Voxel>
        {
            VoxelWorld.GetVoxel(target.WorldPosition + Vector3.forward),
            VoxelWorld.GetVoxel(target.WorldPosition + Vector3.back),
            VoxelWorld.GetVoxel(target.WorldPosition + Vector3.left),
            VoxelWorld.GetVoxel(target.WorldPosition + Vector3.right),
        };
        Voxel min = null;
        foreach (var voxel in voxels)
        {
            if (voxel == null || (voxel.Entity != null && voxel.Entity is Block)) continue;
            var floor = VoxelWorld.GetVoxel(voxel.WorldPosition + Vector3.down);
            if (floor == null ||
                floor.Entity == null ||
                !(floor.Entity is Block)) continue;

            if (min == null ||
                Vector3.Distance(start.WorldPosition, voxel.WorldPosition) <
                Vector3.Distance(start.WorldPosition, min.WorldPosition))
                min = voxel;
        }
        return min;
    }
}
