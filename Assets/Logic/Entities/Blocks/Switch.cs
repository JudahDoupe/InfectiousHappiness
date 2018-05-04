using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : Block, IInteractable
{

    void Start()
    {
        Class = "Block";
        Type = "Switch";
        UpdateMaterial();
    }

    public void Interact(Entity actor)
    {
        StartCoroutine(_Flip(actor));
    }

    private IEnumerator _Flip(Entity actor)
    {
        for (int i = 0; i < 60; i++)
        {
            actor.transform.Rotate(Vector3.forward, 180/60f);
            yield return new WaitForFixedUpdate();
        }
        if (actor is IMovable)
            (actor as IMovable).Fall();
    }
}
