using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;

public class Music : MonoBehaviour
{

    public AudioSource Track1;
    public AudioSource Track2;
    public AudioSource Track3;
    public AudioSource Track4;

    void Update()
    {
        Track2.timeSamples = Track1.timeSamples;
        Track3.timeSamples = Track1.timeSamples;
        Track4.timeSamples = Track1.timeSamples;

        var track2Min = 10;
        var track2Max = 30;
        var track3Min = 50;
        var track3Max = 75;
        var track4Min = 75;
        var track4Max = 90;

        Track2.volume = Mathf.Clamp((Map.TotalInfectedBlocks - track2Min) / track2Max, 0, 1);
        Track3.volume = Mathf.Clamp((Map.TotalInfectedBlocks - track3Min) / track3Max, 0, 1);
        Track4.volume = Mathf.Clamp((Map.TotalInfectedBlocks - track4Min) / track4Max, 0, 1);
    }
}
