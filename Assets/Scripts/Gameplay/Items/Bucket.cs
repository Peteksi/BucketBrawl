using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bucket : ItemBase
{
    [HideInInspector] [Networked] public Vector3 Velocity { get; set; }

    [HideInInspector][Networked] public float StartPositionY { get; set; }

    [HideInInspector] [Networked] public CustomTickTimer FlyTimer { get; set; }

    public AnimationCurve YMotionCurve;


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

        if (Runner.GetPhysicsScene().Raycast(transform.position - normalizedVelocity * .5f,
            normalizedVelocity, out var hitInfo, 1, LayerMask.GetMask("Wall")))
        {
            Velocity = Vector3.Reflect(Velocity, hitInfo.normal);
        }

        transform.position = new(
            transform.position.x,
            StartPositionY + YMotionCurve.Evaluate(FlyTimer.NormalizedValue(Runner)),
            transform.position.z);

        if (FlyTimer.Expired(Runner)) Runner.Despawn(Object);
    }
}


//var inputAuthority = Object.InputAuthority;
//var hitboxManager = Runner.LagCompensation;

//if (hitboxManager.Raycast(transform.position - normalizedVelocity * .5f, normalizedVelocity, 1,
//inputAuthority, out var hitInfo, LayerMask.GetMask("Wall"), HitOptions.IncludePhysX))
