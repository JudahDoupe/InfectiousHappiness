using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProgressTracker : MonoBehaviour
{
    public static ProgressTracker Instance;
    [HideInInspector]
    public List<Block> Blocks = new List<Block>();
    public GameObject ProgressBarParent;
    public GameObject LevelMenu;

    private RectTransform _progressBar;
    private RectTransform _progressBarOutline;
    private bool _tracking;
    private Vector2 _resolution = new Vector2(0,0);

    public void Activate()
    {
        LevelMenu.SetActive(false);
        ProgressBarParent.SetActive(true);
        Reset();
        _tracking = true;
    }
    public void Deactivate()
    {
        ProgressBarParent.SetActive(false);
        Blocks = new List<Block>();
        _tracking = false;
        LevelMenu.SetActive(true);
    }
    public void Reset()
    {
        _progressBar = ProgressBarParent.transform.Find("ProgressBar").GetComponent<RectTransform>();
        _progressBarOutline = ProgressBarParent.transform.Find("ProgressBarOutline").GetComponent<RectTransform>();
        Blocks = new List<Block>();
        foreach (var v in VoxelWorld.ActiveLevel.Voxels)
	    {
	        if (v != null && v.Entity is Block)
	        {
	            Blocks.Add(v.Entity as Block);
	        }
	    }
	}

    void Awake()
    {
        Instance = this;
    }

	void Update () {
        if(!_tracking || Blocks.Count == 0) return;

        var progress = 0f;
	    if (VoxelWorld.ActiveLevel != null && VoxelWorld.ActiveLevel.ActivePuzzle != null)
	        progress = VoxelWorld.ActiveLevel.ActivePuzzle.CompletionFraction;

        _progressBar.localScale = new Vector3(progress, 1, 1);


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
        _progressBar.sizeDelta = new Vector2(width, height); ;
        _progressBar.anchoredPosition = new Vector3(-width / 2f, -(height/2)-padding, 0);

        _progressBarOutline.sizeDelta = new Vector2(width, height); ;
        _progressBarOutline.anchoredPosition = new Vector3(0, -(height/2)-padding, 0);
    }
}
