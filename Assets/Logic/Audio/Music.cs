using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Logic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public List<AudioSource> Tracks = new List<AudioSource>();

    void Start()
    {
        var tracks = transform.GetComponents<AudioSource>();

        foreach (var audioSource in tracks)
        {
            if(!Tracks.Contains(audioSource))
                Tracks.Add(audioSource);
        }
    }

    void Update()
    {
        var baseTrack = Tracks.FirstOrDefault();
        if (!baseTrack) return;
        
        foreach (var track in Tracks)
        {
            track.timeSamples = baseTrack.timeSamples;
        }
    }
}
