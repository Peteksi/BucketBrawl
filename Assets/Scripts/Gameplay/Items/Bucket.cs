using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bucket : ItemBase
{

    // Networked variables

    [HideInInspector] [Networked] public Vector3 Velocity { get; set; }

    [Networked] private float StartPositionY { get; set; }

    [Networked] private CustomTickTimer FlyTimer { get; set; }


    // Local variables

    [SerializeField] float colliderRadius;
    [SerializeField] float colliderLength;

    [SerializeField] float hitBoxRadius;

    [SerializeField] AnimationCurve yMotionCurve;

    List<LagCompensatedHit> hits = new();

    readonly int hitboxLayerMask = 1 << 8;



    // Network methods

    public override void Initialize(Vector3 direction, float speed, float flyTime)
    {
        StartPositionY = transform.position.y;

        Velocity = direction * speed;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);
    }


    public override void FixedUpdateNetwork()
    {
        MoveAndCollide();

        HitboxCheck();

        if (!Object || !Object.IsValid) return;

        if (FlyTimer.Expired(Runner)) Runner.Despawn(Object);
    }


    private void MoveAndCollide()
    {
        transform.position += Velocity * Runner.DeltaTime;

        var normalizedVelocity = Velocity.normalized;

        if (Runner.GetPhysicsScene().SphereCast(transform.position - transform.forward * (colliderLength * .5f),
            colliderRadius, normalizedVelocity, out var hitInfo, colliderLength, LayerMask.GetMask("Wall")))
        {
            Velocity = Vector3.Reflect(Velocity, hitInfo.normal);
        }

        transform.position = new(
            transform.position.x,
            StartPositionY + yMotionCurve.Evaluate(FlyTimer.NormalizedValue(Runner)),
            transform.position.z
        );
    }


    private void HitboxCheck()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var count = hitboxManager.OverlapSphere(transform.position, hitBoxRadius, inputAuthority, hits, layerMask: hitboxLayerMask);

        for (int i = 0; i < count; i++)
        {
            var other = hits[i].GameObject;

            if (other != null && other.TryGetComponent(out IBucketable bucketable) && bucketable.IsBucketable())
            {
                var directionToOther = other.transform.position - transform.position;

                if (Vector3.Dot(directionToOther.normalized, Velocity.normalized) > 0)
                {
                    bucketable.EquipBucket();
                    Runner.Despawn(Object);
                    break;
                }
            }
        }
    }



    // Local methods

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position - .5f * colliderLength * transform.forward, colliderRadius);
        Gizmos.DrawWireSphere(transform.position + .5f * colliderLength * transform.forward, colliderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitBoxRadius);

        Gizmos.color = Color.white;
    }
}