using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Droplet
{
    void Start()
    {
        Class = "Droplet";
        Type = "Water";
        SplashRadius = 2;
    }
}
