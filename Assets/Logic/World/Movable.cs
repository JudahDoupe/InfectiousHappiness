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
    public bool Infectious = false;
    public bool IsCarried;

    private bool _falling;

    void Start()
    {
        Voxel = Map.GetVoxel(transform.position);
        StartingVoxel = Voxel;
    }

    public IEnumerator MoveToVoxel(Voxel vox)
    {

        StartMove();

        var floor = Map.GetVoxel(vox.Position - transform.up);
        if (floor.Block && Infectious)
        {
            floor.Block.Infect();
        }

        while (Vector3.Distance(transform.position, vox.Position) > 0.01)
        {
            transform.position = Vector3.Lerp(transform.position, vox.Position, Speed);
            yield return new WaitForFixedUpdate();
        }

        Land(floor);
    }

    public IEnumerator JumpOnVoxel(Voxel floor)
    {
        StartMove();
        _falling = true;

        var target = Map.GetVoxel(floor.Position + Vector3.up);
        var height = Vector3.Distance(transform.position, floor.Position);


        if (floor.Block && Infectious)
            floor.Block.Infect();

        var frames = Speed * 60;
        for (var t = 0; t < frames; t++)
        {
            var vOffset = (1 - t / frames) * height;
            transform.position = Vector3.Lerp(transform.position, target.Position + transform.up * vOffset, t / frames);

            yield return new WaitForFixedUpdate();

        }

        Land(floor);
    }

    public IEnumerator Fall()
    {
        if(IsCarried)yield break;

        Stunned = true;
        var velocity = 0.0f;

        var potentialFloor = Map.GetVoxel(transform.position - transform.up);
        _falling = true;

        while (potentialFloor.Block == null && Map.InsideMap(transform.position))
        {
            velocity += Map.Gravity;
            transform.Translate(transform.up * -velocity);
            potentialFloor = Map.GetVoxel(transform.position - transform.up);
            yield return new WaitForFixedUpdate();
        }

        if (potentialFloor.Block)
        {
            if(Infectious) potentialFloor.Block.Infect();

            Land(potentialFloor);

        }
        else //died
        {
            StartCoroutine(MoveToVoxel(StartingVoxel));
            var character = transform.GetComponent<Character>();
            if (character)
            {
                character.Drop();
                SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Death);
            }
        }

        Stunned = false;
    }

    private void StartMove()
    {
        Stunned = true;
        Voxel.Block = null;
    }
    private void Land(Voxel floor)
    {

        if (Character.Instance.Voxel == floor)
        {
            Character.Instance._carrying = GetComponent<Block>();
            RecieveLift();
        }
        else if (floor.Block == null)
        {
            StartCoroutine(Fall());
        }
        else if (floor.Block.IsBouncy)
        {
            SoundFX.Instance.PlayClip(SoundFX.Instance.Bounce);
            var v = Voxel.Position - floor.Position;
            var target = Map.GetVoxel(floor.Position - v + 2 * Vector3.Dot(v, transform.up) * transform.up);

            if (target.Block == null)
                StartCoroutine(JumpOnVoxel(Map.GetVoxel(target.Position - transform.up)));
            else
                StartCoroutine(JumpOnVoxel(Map.GetVoxel(Voxel.Position - transform.up)));
        }
        else
        {
            if(_falling && transform.GetComponent<Block>())
                SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Drop);
            _falling = false;
            Voxel = Map.GetVoxel(floor.Position + transform.up);
            transform.position = Voxel.Position;
            Voxel.Block = transform.GetComponent<Block>();
            Stunned = false;
        }

    }

    public void RecievePush()
    {
        if (Stunned) return;

        var direction = Character.Instance.transform.forward;

        var target = Map.GetVoxel(transform.position + direction);

        if (target.Block == null)
        {
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Push);
            StartCoroutine(MoveToVoxel(target));
        }
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
        {
            StartCoroutine(MoveToVoxel(target));
            SoundFX.Instance.PlayRandomClip(SoundFX.Instance.Punch);
            SoundFX.Instance.PlayClip(SoundFX.Instance.PunchVoice);
        }
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
        if (!IsCarried) return false;

        var character = Character.Instance;


        var target = Map.GetVoxel(character.Voxel.Position + character.transform.forward);
        var air = new List<Voxel>
        {
            Map.GetVoxel(character.Voxel.Position + character.transform.up + character.transform.forward),
        };

        if (target.Block == null && air.All(v => v.Block == null))
        {
            IsCarried = false;
            Voxel = Map.GetVoxel(transform.position);
            StartCoroutine(JumpOnVoxel(Map.GetVoxel(target.Position - character.transform.up)));
            transform.parent = null;
            return true;
        }
        return false;

    }
}
