using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BucketBrawl;


public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] NetworkPrefabRef itemPrefab;

    [SerializeField] bool spawnOnStart = false;

    public void SetPrefab(NetworkPrefabRef prefab)
    {
        itemPrefab = prefab;
    }


    public override void Spawned()
    {
        if (spawnOnStart) Spawn();
    }


    public void Spawn(Vector3? direction = null, float speed = 0, float flyHeight = 0, float flyTime = 0)
    {
        if (!Runner.IsServer) return;

        direction ??= transform.forward;

        Runner.Spawn(
            itemPrefab,
            transform.position,
            Quaternion.LookRotation((Vector3)direction),
            Object.InputAuthority,
            (runner, o) =>
            {
                o.GetComponent<ItemBase>().Initialize((Vector3)direction, new ItemBase.FlightParams(speed, flyHeight, flyTime, true));
            }
        );
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new(0, 1, 1, .5f);

        Gizmos.DrawSphere(transform.position, .075f);
        CustomGizmos.DrawCircle(transform.position, transform.up, .5f);
        Gizmos.color = Color.white;
    }
}
