using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityConstructor : MonoBehaviour
{
    public static EntityConstructor Instance;
    public void Awake()
    {
        Instance = this;
    }

    public static Entity NewEntity(string name, string type)
    {
        Entity e;
        switch (name)
        {
            case "Block":
                e = NewBlock(type);
                break;
            case "Droplet":
                e = NewDroplet(type);
                break;
            default:
                Debug.Log(name+" "+type+" entity not supported");
                e =  new GameObject("").AddComponent<Entity>();
                break;
        }
        e.transform.name = type+name;
        return e;
    }
    public static Block NewBlock(string type)
    {
        var obj = Instantiate(Resources.Load<GameObject>("Entities/Block"), new Vector3(0,0,0), Quaternion.identity);
        Block block;

        switch (type)
        {
            case "Static":
                block = obj.AddComponent<Static>();
                break;
            case "Undyed":
                block = obj.AddComponent<Undyed>();
                break;
            case "Movable":
                block = obj.AddComponent<Movable>();
                break;
            case "Cloud":
                block = obj.AddComponent<Cloud>();
                break;
            case "Goal":
                block = obj.AddComponent<Goal>();
                break;
            case "Bounce":
                block = obj.AddComponent<Bounce>();
                break;
            case "Dispenser":
                block = obj.AddComponent<Dispenser>();
                break;
            default:
                block = obj.AddComponent<Block>();
                break;
        }

        return block;
    }
    public static Droplet NewDroplet(string type)
    {
        var obj = Instantiate(Resources.Load<GameObject>("Entities/Droplet"), new Vector3(0, 0, 0), Quaternion.identity);
        var droplet = obj.AddComponent<Droplet>();
        droplet.Type = type;
        return droplet;
    }
}