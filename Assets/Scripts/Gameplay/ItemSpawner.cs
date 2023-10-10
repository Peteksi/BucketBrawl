using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : SimulationBehaviour
{
    [SerializeField] NetworkPrefabRef itemPrefab;


    public void SetPrefab(NetworkPrefabRef prefab)
    {
        itemPrefab = prefab;
    }


    public void Spawn(Vector3? direction = null, float speed = 0)
    {
        if (direction == null) direction = transform.forward;

        Runner.Spawn(
            itemPrefab,
            transform.position,
            Quaternion.LookRotation((Vector3)direction),
            Object.InputAuthority,
            (runner, o) =>
            {
                o.GetComponent<ItemBase>().Initialize((Vector3)direction, speed);
            }
        );
    }
}
