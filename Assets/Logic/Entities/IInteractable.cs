using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void InteractPrimary(Entity actor);
    void InteractSecondary(Entity actor);
}
