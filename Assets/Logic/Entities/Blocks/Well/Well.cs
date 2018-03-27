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
        if(!IsDyed)
            Dye();

        if (_springTimer == 0 && SpringVox.Entity == null && DropletType != "")
            SpringVox.Fill(EntityConstructor.NewDroplet(DropletType));

        _springTimer = (_springTimer + 1) % (SpringInterval * 60);
    }
}
