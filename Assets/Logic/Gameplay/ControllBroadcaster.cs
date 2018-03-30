using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControllBroadcaster : MonoBehaviour {

    public LayerMask LayerMask;

    private Touch? _touch = null;

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
        foreach (var t in Input.touches)
        {
            if (!_touch.HasValue && t.phase == TouchPhase.Began)
                _touch = t;
        }

        switch (_touch.Value.phase)
        {
            case TouchPhase.Began:
                StartTouch(_touch.Value.position);
                break;
            case TouchPhase.Moved:
                MoveTouch(_touch.Value.position);
                break;
            case TouchPhase.Stationary:
                break;
            case TouchPhase.Ended:
                EndTouch(_touch.Value.position);
                break;
            case TouchPhase.Canceled:
                EndTouch(_touch.Value.position);
                break;
            default:
                break;
        }
    }

    public void StartTouch(Vector2 screenPos)
    {
    }
    public void MoveTouch(Vector2 screenPos)
    {
    }
    public void EndTouch(Vector2 screenPos)
    {
        var vox = GetVoxel(screenPos);
        var player = VoxelWorld.MainCharacter;
        if (vox == null || player == null) return;

        if (player.Load != null)
        {
            player.Throw(vox);
            return;
        }

        if(vox.Entity is Block)
            vox = VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.up);

        if (!(vox.Entity is Block))
        {
            player.MoveAlongPath(Pathfinder.FindPath(player.Voxel,vox).ToArray());
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
}
