using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;

public class DevLevel : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Build();
	}

    void Build()
    {
        var l = new Level(new Vector3(-25,-25,-1));
        l.StartingVoxel = l.GetVoxel(new Vector3(25, 25, 1));

        Map.Levels.Add(l);
        Map.CurrentLevel = l;
    }
}
