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

    public Material UndyedBlockMaterial;
    public List<TypeMaterial> BlockMaterials = new List<TypeMaterial>();
    public List<TypeMaterial> DropletMaterials = new List<TypeMaterial>();

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
            case "Moveable":
                block = obj.AddComponent<Moveable>();
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

        block.DyeMaterial = Instance.BlockMaterials.FirstOrDefault(x => x.Type == type).Material;
        obj.GetComponentInChildren<Renderer>().material = Instance.UndyedBlockMaterial;
        return block;
    }
    public static Droplet NewDroplet(string type)
    {
        var obj = Instantiate(Resources.Load<GameObject>("Entities/Droplet"), new Vector3(0, 0, 0), Quaternion.identity);

        switch (type)
        {
            case "Dye":
                obj.AddComponent<Dye>();
                break;
            case "Water":
                obj.AddComponent<Water>();
                break;
            case "Fire":
                obj.AddComponent<Fire>();
                break;
            default:
                obj.AddComponent<Droplet>();
                break;
        }

        obj.GetComponentInChildren<Renderer>().material = Instance.DropletMaterials.FirstOrDefault(x => x.Type == type).Material;
        return obj.GetComponent<Droplet>();
    }
}

[Serializable]
public struct TypeMaterial
{
    public string Type;
    public Material Material;
}