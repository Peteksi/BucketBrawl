using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bucket : ItemBase
{
    [HideInInspector][Networked] public Vector3 Velocity { get; set; }

    [Networked] private float StartPositionY { get; set; }

    [Networked] private CustomTickTimer FlyTimer { get; set; }

    [SerializeField] float sphereCastRadius;
    [SerializeField] float sphereCastLength;

    [SerializeField] AnimationCurve yMotionCurve;


    public override void Initialize(Vector3 direction, float speed, float flyTime)
    {
        StartPositionY = transform.position.y;

        Velocity = direction * speed;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);
    }


    public override void FixedUpdateNetwork()
    {
        transform.position += Velocity * Runner.DeltaTime;

        var normalizedVelocity = Velocity.normalized;

        if (Runner.GetPhysicsScene().SphereCast(transform.position - transform.forward * (sphereCastLength * .5f),
            sphereCastRadius, normalizedVelocity, out var hitInfo, sphereCastLength, LayerMask.GetMask("Wall")))
        {
            Velocity = Vector3.Reflect(Velocity, hitInfo.normal);
        }

        transform.position = new(
            transform.position.x,
            StartPositionY + yMotionCurve.Evaluate(FlyTimer.NormalizedValue(Runner)),
            transform.position.z);

        if (FlyTimer.Expired(Runner)) Runner.Despawn(Object);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - transform.forward * sphereCastLength * .5f, sphereCastRadius);
        Gizmos.DrawWireSphere(transform.position + transform.forward * sphereCastLength * .5f, sphereCastRadius);
        Gizmos.color = Color.white;
    }
}


//var inputAuthority = Object.InputAuthority;
//var hitboxManager = Runner.LagCompensation;

//if (hitboxManager.Raycast(transform.position - normalizedVelocity * .5f, normalizedVelocity, 1,
//inputAuthority, out var hitInfo, LayerMask.GetMask("Wall"), HitOptions.IncludePhysX))
