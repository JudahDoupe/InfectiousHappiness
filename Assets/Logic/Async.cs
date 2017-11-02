using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic;
using Assets.Logic.Framework;
using JetBrains.Annotations;
using UnityEngine;

public class Async : MonoBehaviour
{
    public static Async Instance;

    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public void RandomlyActivateBlocks(List<Block> blocks)
    {
        StartCoroutine(randomlyActivateBlocks(blocks));
    }
    private IEnumerator randomlyActivateBlocks(List<Block> blocks)
    {
        blocks = blocks.OrderBy(x => Random.Range(0, 100)).ToList();
        var i = 0;
        var speed = 5;
        foreach (var block in blocks)
        {
            block.Activate();
            if (i == 0)
                yield return new WaitForFixedUpdate();
            i = (i + 1) % speed;
        }
    }

    public void AdjustTrackVolume(Room room, AudioSource track)
    {
        StartCoroutine(adjustTrackVolume(room,track));
    }
    private IEnumerator adjustTrackVolume(Room room, AudioSource track)
    {
        while (!room.IsComplete)
        {
            track.volume = room.Floor.Count(x => x.IsActivated) / (room.Floor.Count + 0f);
            yield return new WaitForSeconds(1);
        }
        track.volume = 1;
    }

}
