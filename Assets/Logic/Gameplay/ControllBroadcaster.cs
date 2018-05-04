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
    public static bool IsActive = true;

    private Vector2? _touchOrigin;
    private Vector2? _touchPos;

    public void Update()
    {
        if(!IsActive)return;
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

        VoxelWorld.MainCamera.Height -= (screenPos.y - _touchPos.Value.y) * 0.0025f;
        VoxelWorld.MainCamera.Rotate((_touchOrigin.Value.y > Screen.height / 3.5f ? -(screenPos.x - _touchPos.Value.x) : screenPos.x - _touchPos.Value.x)*0.3f);

        _touchPos = screenPos;
    }
    public void EndTouch(Vector2 screenPos)
    {
        _touchPos = null;
        if (Vector2.Distance(screenPos, _touchOrigin.Value) > TouchMovementTolerance) return;
 
        var vox = GetVoxel(screenPos);
        var player = VoxelWorld.MainCharacter;

        if (player == null || vox == null || vox.Entity == null) return;

        if (vox.Entity is IInteractable)
        {
            player.FollowPath(Pathfinder.FindPath(player.Voxel, GetNearestWalkableVoxel(vox, player.Voxel)).ToArray());
            StartCoroutine(Interact(player, vox.Entity as IInteractable));
        }
        else if (player.Load != null && vox.Entity.IsActive)
        {
            player.Throw(vox);
        }
        else
        {
            var topVox = VoxelWorld.GetVoxel(vox.WorldPosition + VoxelWorld.MainCharacter.transform.up);
            player.FollowPath(Pathfinder.FindPath(player.Voxel, topVox).ToArray());
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
            var floor = VoxelWorld.GetVoxel(voxel.WorldPosition - VoxelWorld.MainCharacter.transform.up);
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


    private IEnumerator Interact(Character player, IInteractable entity)
    {
        while (player.IsMoving())
            yield return new WaitForFixedUpdate();
        entity.Interact(player);
    }
}
