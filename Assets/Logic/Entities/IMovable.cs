using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{
    void Reset();
    void Fall();

    void ArchTo(Voxel vox, bool forceMove = false);
    void MoveTo(Voxel vox, bool forceMove = false);
    void MoveAlongPath(Voxel[] path, bool forceMove = true);
}
