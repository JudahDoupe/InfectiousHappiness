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

        var track2Min = 0;
        var track2Max = Map.CurrentLevel.Blocks * 2 / 5;
        var track3Min = Map.CurrentLevel.Blocks * 1 / 5;
        var track3Max = Map.CurrentLevel.Blocks * 3 / 5;
        var track4Min = Map.CurrentLevel.Blocks * 2 / 5;
        var track4Max = Map.CurrentLevel.Blocks * 4 / 5;

        Track2.volume = Mathf.Clamp((Map.CurrentLevel.ActiveBlocks - track2Min) / track2Max, 0, 1);
        Track3.volume = Mathf.Clamp((Map.CurrentLevel.ActiveBlocks - track3Min) / track3Max, 0, 1);
        Track4.volume = Mathf.Clamp((Map.CurrentLevel.ActiveBlocks - track4Min) / track4Max, 0, 1);
    }
}
