using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 Position = Vector3.zero;
    public float Distance = 10;
    public float Height = 0.5f;
    public float Angle = 0;

    private float _height = 1;
    private float _angle = 0;

    void FixedUpdate()
    {
        Position = VoxelWorld.MainCharacter.transform.position;
        _height = Mathf.Lerp(_height, Height, Time.deltaTime * 10);
        _angle = Mathf.Lerp(_angle, Angle, Time.deltaTime * 10);

        var direction = new Vector3(0, _height, _height * _height - 1);
        direction = Quaternion.AngleAxis(_angle, Vector3.up) * direction;
        direction.Normalize();
        transform.position = Position + direction * Distance;

        transform.LookAt(Position);
    }

    public void CenterOn(Vector3 position, float distance)
    {
        Position = position;
        Distance = distance;
    }
    public void Move(Vector2 direction)
    {
        Height = Mathf.Clamp(Height - direction.y,0.001f,0.999f);
        Angle = (Angle + direction.x * 45) % 360;
    }
}
