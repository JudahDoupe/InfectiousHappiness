using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StyleDictionary : MonoBehaviour
{
    public static StyleDictionary Instance;
    public List<BlockStyleSet> BlockStyles;

    public void Awake()
    {
        if(Instance == null)Instance = this;
    }

    public static BlockStyleSet GetBlockStyleSet(BlockType type)
    {
        Debug.Log(type);
        return Instance.BlockStyles.FirstOrDefault(x => x.BlockType == type);
    }
}

[Serializable]
public struct BlockStyleSet
{
    public BlockType BlockType;
    public Material InactiveMaterial;
    public Material ActiveMaterial;
}