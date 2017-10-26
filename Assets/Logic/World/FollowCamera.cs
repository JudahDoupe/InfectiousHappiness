using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public Character Character;
    public float Setback = 3;
    public float Height = 6;
    public float Speed = 0.2f;

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            Height += Speed;
        if (Input.GetKey(KeyCode.DownArrow))
            Height -= Speed;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            Character.TurnLeft();
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Character.TurnRight();

        Height = Mathf.Clamp(Height, 0, 15);

        var newPos = Character.transform.position
                     + (Character.transform.forward * -Setback)
                     + (Character.transform.up * Height);
        transform.position = Vector3.Lerp(transform.position,newPos, Speed/4);

        Vector3 direction = (Character.transform.position + Character.transform.up * Height / 2) - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Speed);
    }
}
