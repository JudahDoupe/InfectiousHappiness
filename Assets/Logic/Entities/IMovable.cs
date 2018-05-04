using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{
    bool IsMoving();

    void Reset();
    void Fall();

    void MoveTo(Voxel vox, bool moveThroughBlocks = false, bool enterVoxel = true);
    void FollowPath(Voxel[] path, bool moveThroughBlocks = true);
}
