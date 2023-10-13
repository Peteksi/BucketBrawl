using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bucket : ItemBase
{
    [HideInInspector] [Networked] public Vector3 Velocity { get; set; }

    public AnimationCurve YMotionCurve;


    public override void Initialize(Vector3 direction, float speed)
    {
        Velocity = direction * speed;
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
    }
}


//var inputAuthority = Object.InputAuthority;
//var hitboxManager = Runner.LagCompensation;

//if (hitboxManager.Raycast(transform.position - normalizedVelocity * .5f, normalizedVelocity, 1,
//inputAuthority, out var hitInfo, LayerMask.GetMask("Wall"), HitOptions.IncludePhysX))
