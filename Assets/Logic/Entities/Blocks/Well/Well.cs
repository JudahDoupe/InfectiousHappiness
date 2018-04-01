using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Well : Block
{
    public int SpringInterval = 1;
    public string DropletType = "";
    public Voxel SpringVox;

    private int _springTimer = 0;

    public void UpdateSpring()
    {
        IsDyed = true;

        if (_springTimer == 0 && SpringVox.Entity == null && DropletType != "")
            SpringVox.Fill(EntityConstructor.NewDroplet(DropletType),Voxel.Puzzle.Number);

        _springTimer = (_springTimer + 1) % (SpringInterval * 60);
    }
}
