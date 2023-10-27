using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : SimulationBehaviour
{
    [SerializeField] NetworkPrefabRef itemPrefab;

    [SerializeField] float offset;

    public void SetPrefab(NetworkPrefabRef prefab)
    {
        itemPrefab = prefab;
    }


    public void Spawn(Vector3? direction = null, float speed = 0, float flyTime = 0)
    {
        if (!Runner.IsServer) return;

        if (direction == null) direction = transform.forward;

        Runner.Spawn(
            itemPrefab,
            transform.position + transform.forward * offset,
            Quaternion.LookRotation((Vector3)direction),
            Object.InputAuthority,
            (runner, o) =>
            {
                o.GetComponent<ItemBase>().Initialize((Vector3)direction, speed, flyTime);
            }
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward * offset, .1f);
        Gizmos.color = Color.white;
    }
}
