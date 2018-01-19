using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{

    public float Setback = 3;
    public float Height = 6;
    public float Speed = 0.2f;
    private Character _character;

    void Start()
    {
        _character = VoxelWorld.Instance.MainCharacter;
    }
    void FixedUpdate()
    {
        Height = Mathf.Clamp(Height, 0, 15);

        var newPos = _character.transform.position
                     + (_character.transform.forward * -Setback)
                     + (_character.transform.up * Height);
        transform.position = Vector3.Lerp(transform.position,newPos, Speed/4);

        Vector3 direction = (_character.transform.position + _character.transform.up * Height / 2) - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction, _character.transform.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Speed);
    }
}
