using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Droplet
{
    void Start()
    {
        Class = "Droplet";
        Type = "Fire";
        SplashRadius = 1;
    }
}
