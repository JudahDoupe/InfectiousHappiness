using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{
    bool IsMoving();

    void Reset();
    void Fall();

    void MoveTo(Voxel vox, bool forceMove = false);
    void FollowPath(Voxel[] path, bool forceMove = true);
}
