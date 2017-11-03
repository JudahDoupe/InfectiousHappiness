using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic;
using UnityEngine;

public class Music : MonoBehaviour
{
    private readonly List<AudioSource> _tracks = new List<AudioSource>();

    void Update()
    {
        var baseTrack = _tracks.FirstOrDefault();
        if (!baseTrack) return;
        
        foreach (var track in _tracks)
        {
            track.timeSamples = baseTrack.timeSamples;
        }
    }

    public AudioSource AddTrack(AudioClip track, Room room)
    {
        if (!enabled) return null;

        var src = gameObject.AddComponent<AudioSource>();
        src.clip = track;
        src.loop = true;
        src.Play();
        _tracks.Add(src);
        StartCoroutine(AdjustTrackVolume(src, room));
        return src;
    }

    private IEnumerator AdjustTrackVolume(AudioSource track, Room room)
    {
        while (!room.IsComplete)
        {
            track.volume = room.Floor.Count(x => x.IsActivated) / (room.Floor.Count + 0f);
            yield return new WaitForSeconds(1);
        }
        track.volume = 1;
    }
}
