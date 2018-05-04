using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Speed = 5;
    public float Distance = 10;
    public float Height = 0.33f;

    private Quaternion _targetRotation = Quaternion.identity;

    void LateUpdate()
    {
        var position = VoxelWorld.MainCharacter.transform.position + VoxelWorld.MainCharacter.transform.up * Distance / 3.5f ;
        transform.parent.position = Vector3.Lerp(transform.parent.position, position, Time.deltaTime * Speed);
        transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, _targetRotation, Time.deltaTime * Speed);

        Height = Mathf.Clamp(Height, 0.2f, 0.8f);
        var direction = new Vector3(0, Height * VoxelWorld.MainCharacter.transform.up.y, Height * Height - 1).normalized;
        var targetPosition = direction * Distance;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * Speed);
        transform.LookAt(transform.parent.position,VoxelWorld.MainCharacter.transform.up);
    }

    public void Rotate(float degrees)
    {
        _targetRotation *= Quaternion.Euler(VoxelWorld.MainCharacter.transform.up * degrees); ;
    }
    public void LookAt(Vector3 target)
    {
        target.y = VoxelWorld.MainCharacter.transform.position.y;
        _targetRotation.SetLookRotation(target - VoxelWorld.MainCharacter.transform.position, VoxelWorld.MainCharacter.transform.up);
    }

    public void CinematicLookAt(Vector3 target)
    {
        StartCoroutine(cinematicLookAt(target));
    }

    private IEnumerator cinematicLookAt(Vector3 target)
    {
        ControllBroadcaster.IsActive = false;
        Speed = Speed / 3;
        LookAt(target);
        while (Quaternion.Angle(_targetRotation, transform.parent.rotation) > 10)
        {
            yield return new WaitForEndOfFrame();
        }
        ControllBroadcaster.IsActive = true;
        while (Quaternion.Angle(_targetRotation, transform.parent.rotation) > 0.5f)
        {
            yield return new WaitForEndOfFrame();
        }
        Speed = Speed * 3;
    }
}
