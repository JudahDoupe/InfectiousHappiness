using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance;
    public List<BlockMaterialSet> BlockMaterials;

    public void Awake()
    {
        if(Instance == null)Instance = this;
    }

    public static BlockMaterialSet GetBlockMaterials(BlockType type, int variation = 0)
    {
        for (var i = 0; i < Instance.BlockMaterials.Count; i++)
        {
            if (Instance.BlockMaterials[i].BlockType == type && variation == Instance.BlockMaterials[i].Variation)
                return Instance.BlockMaterials[i];
        }
        for (var i = 0; i < Instance.BlockMaterials.Count; i++)
        {
            if (Instance.BlockMaterials[i].BlockType == type)
                return Instance.BlockMaterials[i];
        }
        return Instance.BlockMaterials.FirstOrDefault();
    }
}

[Serializable]
public struct BlockMaterialSet
{
    public BlockType BlockType;
    public int Variation;
    public Material InactiveMaterial;
    public Material ActiveMaterial;
}