using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic.World;
using UnityEngine;

public class Movable : MonoBehaviour {

    public Voxel Voxel;
    public Voxel StartingVoxel;
    public float Speed = 0.25f;
    public bool Stunned;
    public bool Infections = false;
    public bool IsCarried;

    void Start()
    {
        Voxel = Map.GetVoxel(transform.position);
        StartingVoxel = Voxel;
    }

    public IEnumerator MoveToVoxel(Voxel vox)
    {
        Stunned = true;
        Voxel.Block = null;
        Voxel = vox;

        var floor = Map.GetVoxel(vox.Position + Vector3.down).Block;
        if (floor && Infections)
        {
            floor.Infect();
        }

        while (Vector3.Distance(transform.position, vox.Position) > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, vox.Position, Speed);
            yield return new WaitForFixedUpdate();
        }

        transform.position = vox.Position;
        Voxel.Block = transform.GetComponent<Block>();

        if (floor == null)
            StartCoroutine(Fall());


        Stunned = false;
    }

    public IEnumerator JumpOnVoxel(Voxel vox)
    {
        Stunned = true;
        Voxel.Block = null;
        Voxel = Map.GetVoxel(vox.Position + Vector3.up);
        var height = Vector3.Distance(transform.position, vox.Position);

        var floor = Map.GetVoxel(vox.Position).Block;
        if (floor && Infections)
            floor.Infect();

        var frames = Speed * 60;
        for (var t = 0; t < frames; t++)
        {
            var vOffset = (1 - t / frames) * height;
            transform.position = Vector3.Lerp(transform.position, Voxel.Position + transform.up * vOffset, t / frames);

            yield return new WaitForFixedUpdate();

        }

        transform.position = Voxel.Position;

        if (floor == null)
            StartCoroutine(Fall());

        Map.GetVoxel(transform.position).Block = transform.GetComponent<Block>();

        Stunned = false;

    }

    public IEnumerator Fall()
    {
        if(IsCarried)yield break;

        Stunned = true;
        var velocity = 0.0f;

        var block = Map.GetVoxel(Voxel.Position - transform.up).Block;

        while (block == null && Map.InsideMap(transform.position))
        {
            velocity += Map.Gravity;
            transform.Translate(transform.up * -velocity);
            Voxel = Map.GetVoxel(transform.position);
            block = Map.GetVoxel(Voxel.Position - transform.up).Block;
            yield return new WaitForFixedUpdate();
        }

        if (block)
        {
            if(Infections) block.Infect();

            var dist = Vector3.Distance(transform.position, Voxel.Position);
            var lastDist = dist;
            while (lastDist <= dist)
            {
                velocity += Map.Gravity;
                transform.Translate(transform.up * -velocity);
                lastDist = dist;
                dist = Vector3.Distance(transform.position, Voxel.Position);
                yield return new WaitForFixedUpdate();
            }
            transform.position = Voxel.Position;
        }
        else //died
        {
            StartCoroutine(MoveToVoxel(StartingVoxel));
        }

        Stunned = false;
    }

    public void RecievePush()
    {
        if (Stunned) return;

        var direction = Character.Instance.transform.forward;

        var target = Map.GetVoxel(transform.position + direction);

        if (target.Block == null)
            StartCoroutine(MoveToVoxel(target));
    }
    public void RecievePunch()
    {
        if (Stunned) return;

        var direction = Character.Instance.transform.forward;

        var target = Map.GetVoxel(transform.position + direction * 2);
        var air = new List<Voxel>
        {
            Map.GetVoxel(transform.position + direction),
        };

        if (target.Block == null && air.All(v => v.Block == null))
            StartCoroutine(MoveToVoxel(target));
    }
    public bool RecieveLift()
    {
        if (Stunned || IsCarried) return false;


        var character = Character.Instance;

        IsCarried = true;

        var target = Map.GetVoxel(character.Voxel.Position + character.transform.up);
        var air = new List<Voxel>
        {
            Map.GetVoxel(character.Voxel.Position + character.transform.up + character.transform.forward),
        };

        if (target.Block == null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(Map.GetVoxel(target.Position - character.transform.up)));

        transform.parent = Character.Instance.transform;

        return true;
    }
    public bool RecieveDrop()
    {
        if (Stunned || !IsCarried) return false;

        var character = Character.Instance;

        IsCarried = false;
        Voxel = Map.GetVoxel(transform.position);
        Voxel.Block = transform.GetComponent<Block>();

        var target = Map.GetVoxel(character.Voxel.Position + character.transform.forward);
        var air = new List<Voxel>
        {
            Map.GetVoxel(character.Voxel.Position + character.transform.up + character.transform.forward),
        };

        if (target.Block == null && air.All(v => v.Block == null))
            StartCoroutine(JumpOnVoxel(Map.GetVoxel(target.Position - character.transform.up)));

        transform.parent = null;

        return true;
    }
}
