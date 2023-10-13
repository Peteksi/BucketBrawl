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
    }
}
