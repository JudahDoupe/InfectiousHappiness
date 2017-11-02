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
        Tracks.AddRange(transform.GetComponents<AudioSource>());
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
