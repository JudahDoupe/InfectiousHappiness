using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Block {

    void Start()
    {
        Class = "Block";
        Type = "Goal";
        UpdateMaterial();
    }

    public override void Collide(Droplet droplet)
    {
        if(droplet.Type == Type)
            Voxel.Puzzle.IsComplete = true;
        Destroy(droplet.gameObject);
    }
}
