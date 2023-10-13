using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : NetworkBehaviour
{
    public abstract void Initialize(Vector3 direction, float speed);
}
