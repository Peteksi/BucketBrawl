using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEditor;

public class Bucket : ItemBase
{

    // Local variables

    [SerializeField] float hitBoxRadius;

    List<LagCompensatedHit> hits = new();

    readonly int playerLayerMask = 1 << 6;



    // Network methods

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (CurrentState == (int)State.Flying)
        {
            HitboxQuery();
        }
    }


    private void HitboxQuery()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var count = hitboxManager.OverlapSphere(transform.position, hitBoxRadius, inputAuthority, hits,
            layerMask: playerLayerMask);

        for (int i = 0; i < count; i++)
        {
            var other = hits[i].GameObject;

            if (other != null && other.TryGetComponent(out IBucketable bucketable) && bucketable.IsBucketable())
            {
                var directionToOther = other.transform.position - transform.position;

                if (Vector3.Dot(directionToOther.normalized, Velocity.normalized) > 0)
                {
                    bucketable.EquipBucket(this);
                    CurrentState = (int)State.Inactive;
                    break;
                }
            }
        }
    }



    // Local methods

    private void OnDrawGizmos()
    {
        base.DrawGizmos();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitBoxRadius);
        Gizmos.color = Color.white;
    }
}