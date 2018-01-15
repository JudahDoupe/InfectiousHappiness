using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public UIManager ScreenText;
    public TutorialState[] States;

    public void Update()
    {
        for (int i = 0; i < States.Length; i++)
        {
            if (!States[i].Complete && VoxelWorld.ActiveLevel.Name == States[i].LevelName)
            {
                if (Vector3.Distance(VoxelWorld.Instance.MainCharacter.transform.position,
                        VoxelWorld.ActiveLevel.LevelToWorld(States[i].LevelPosition)) < 0.1)
                {
                    ScreenText.ShowText(States[i].Text, States[i].DisplayTimeInSeconds);
                    States[i].Active = true;
                }
                else if (States[i].Active)
                {
                    ScreenText.ClearText();
                    States[i].Complete = true;
                    States[i].Active = false;
                }
            }
        }
    }
}

[Serializable]
public struct TutorialState
{
    public string Name;
    public string Text;
    public float DisplayTimeInSeconds;
    public string LevelName;
    public bool Active;
    public bool Complete;
    public Vector3 LevelPosition;
}