using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;
    public int MaxDistance;

    void Awake()
    {
        Instance = this;
    }

    public static List<Voxel> FindPath(Voxel start, Voxel end)
    {
        if (start == null || end == null) return new List<Voxel>();

        var evaluatedNodes = new List<PFNode>();
        var unevaluatedNodes = new List<PFNode>();
        var cNode = new PFNode{Voxel = start,Last = null, Distance = 1};
        unevaluatedNodes.Add(cNode);

        while (cNode.Voxel != end && unevaluatedNodes.Count > 0)
        {
            cNode = GetMinCost(unevaluatedNodes,end);
            unevaluatedNodes.Remove(cNode);
            evaluatedNodes.Add(cNode);

            if (cNode.Distance >= Instance.MaxDistance) continue;

            var neighbors = GetMovableNeighbors(cNode.Voxel);
            foreach (var vox in neighbors)
            {
                if (evaluatedNodes.Any(x => x.Voxel == vox)) continue;
                var node = new PFNode { Last = cNode, Voxel = vox, Distance = cNode.Distance + 1};
                unevaluatedNodes.Add(node);
            }
        }

        var path = new List<Voxel>();
        if (cNode.Voxel != end) return path;

        while (cNode.Last != null)
        {
            path.Add(cNode.Voxel);
            cNode = cNode.Last;
        }
        path.Reverse();
        return path;
    }
    private static List<Voxel> GetMovableNeighbors(Voxel vox)
    {
        var rtn = new List<Voxel>();
        var voxels = new List<Voxel>
        {
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.forward),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.back),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.left),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.right),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.forward + Vector3.down),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.back + Vector3.down),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.left + Vector3.down),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.right + Vector3.down),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.forward + Vector3.up),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.back + Vector3.up),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.left + Vector3.up),
            VoxelWorld.GetVoxel(vox.WorldPosition + Vector3.right + Vector3.up),

        };
        foreach (var voxel in voxels)
        {
            if(voxel == null || (voxel.Entity != null && voxel.Entity is Block))continue;
            var floor = VoxelWorld.GetVoxel(voxel.WorldPosition + Vector3.down);
            if (floor == null ||
                floor.Entity == null ||
                !(floor.Entity is Block)) continue;

            rtn.Add(voxel);
        }
        return rtn;
    }
    private static PFNode GetMinCost(List<PFNode> nodes, Voxel end)
    {
        PFNode min = new PFNode {Voxel = end, Distance = Int32.MaxValue};
        foreach (var pfNode in nodes)
        {
            if (pfNode.Cost(end) < min.Cost(end))
                min = pfNode;
        }
        return min;
    }
}
public class PFNode
{
    public float Cost(Voxel end)
    {
        return Distance + Vector3.Distance(Voxel.WorldPosition, end.WorldPosition);
    }
    public int Distance;
    public Voxel Voxel;
    public PFNode Last;
}
