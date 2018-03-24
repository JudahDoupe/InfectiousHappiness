using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : Block, IMovable {

    void Start()
    {
        Class = "Block";
        Type = "Moveable";
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public void Fall()
    {
        throw new System.NotImplementedException();
    }

    public bool MovePathClear()
    {
        throw new System.NotImplementedException();
    }

    public void MoveTo(Voxel vox, bool forceMove = false)
    {
        throw new System.NotImplementedException();
    }

    public void MoveAlongPath(Voxel[] path, bool forceMove = true)
    {
        throw new System.NotImplementedException();
    }

    public bool ArchPathClear()
    {
        throw new System.NotImplementedException();
    }

    public void ArchTo(Voxel vox, bool forceMove = false)
    {
        throw new System.NotImplementedException();
    }
}
