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
            case "Movable":
                block = obj.AddComponent<Movable>();
                break;
            case "Cloud":
                block = obj.AddComponent<Cloud>();
                break;
            case "DyeWell":
                block = obj.AddComponent<DyeWell>();
                break;
            case "WaterWell":
                block = obj.AddComponent<WaterWell>();
                break;
            case "FireWell":
                block = obj.AddComponent<FireWell>();
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
        Droplet droplet;

        switch (type)
        {
            case "Dye":
                droplet = obj.AddComponent<Dye>();
                break;
            case "Water":
                droplet = obj.AddComponent<Water>();
                break;
            case "Fire":
                droplet = obj.AddComponent<Fire>();
                break;
            default:
                droplet = obj.AddComponent<Droplet>();
                break;
        }

        return obj.GetComponent<Droplet>();
    }
}