using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProgressTracker : MonoBehaviour
{

    public float CompletionFraction = 0.75f;
    [HideInInspector]
    public List<Block> Blocks = new List<Block>();
    public RectTransform ProgressBar;
    public RectTransform ProgressBarOutline;
    public RectTransform ProgressBarMarker;

    private Vector2 _resolution = new Vector2(0,0);

    void Start () {
        foreach (var v in VoxelWorld.ActiveLevel.Voxels)
	    {
	        if (v != null && v.Entity is Block)
	        {
	            Blocks.Add(v.Entity as Block);
	        }
	    }
	}
	
	void Update () {
	    ProgressBar.localScale = new Vector3(Blocks.Count(x => x.IsDyed) / (float)Blocks.Count,1,1);

	    if (Math.Abs(_resolution.y - Screen.height) > 1 || Math.Abs(_resolution.x - Screen.width) > 1)
	    {
	        _resolution = new Vector2(Screen.width, Screen.height);
	        UpdateProgressBarPosition();
	    }
    }

    public void UpdateProgressBarPosition()
    {
        var padding = 10;
        var width = _resolution.x - padding;
        var height = _resolution.y * 0.05f;

        var newRect = new Rect(0, -height + padding, width, height);
        ProgressBar.sizeDelta = new Vector2(width, height); ;
        ProgressBar.anchoredPosition = new Vector3(-width / 2f, -(height/2)-padding, 0);

        ProgressBarOutline.sizeDelta = new Vector2(width, height); ;
        ProgressBarOutline.anchoredPosition = new Vector3(0, -(height/2)-padding, 0);

        ProgressBarMarker.sizeDelta = new Vector2(height / 2, height);
        ProgressBarMarker.anchoredPosition = new Vector3((-width / 2f) + (width * CompletionFraction), -(height / 2) - padding, 0);
    }
}
