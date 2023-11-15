using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] NetworkPrefabRef itemPrefab;

    [SerializeField] float zOffset;

    [SerializeField] bool spawnOnStart = false;

    public void SetPrefab(NetworkPrefabRef prefab)
    {
        itemPrefab = prefab;
    }


    public override void Spawned()
    {
        if (spawnOnStart) Spawn();
    }


    public void Spawn(Vector3? direction = null, float speed = 0, float flyTime = 0)
    {
        if (!Runner.IsServer) return;

        if (direction == null) direction = transform.forward;

        Runner.Spawn(
            itemPrefab,
            transform.position + transform.forward * zOffset,
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + transform.forward * zOffset, .1f);
        Gizmos.color = Color.white;
    }
}
