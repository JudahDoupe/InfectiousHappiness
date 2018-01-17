using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public UpgradeType Type;

    public void SetType(string typeName)
    {
        switch (typeName)
        {
            case "Jump":
                Type = UpgradeType.Jump;
                break;
            case "Push":
                Type = UpgradeType.Push;
                break;
            case "Lift":
                Type = UpgradeType.Lift;
                break;
            case "Pipe":
                Type = UpgradeType.Pipe;
                break;
            case "Switch":
                Type = UpgradeType.Switch;
                break;
            default:
                Type = UpgradeType.Jump;
                break;
        }
    }
}

public enum UpgradeType
{
    Jump,
    Push,
    Lift,
    Pipe,
    Switch,
}