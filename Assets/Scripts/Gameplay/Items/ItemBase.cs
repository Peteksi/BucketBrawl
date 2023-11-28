using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BucketBrawl;
using UnityEngine.UIElements;


public class ItemBase : NetworkBehaviour
{

    // Networked variables

    [HideInInspector] [Networked] public Vector3 Velocity { get; set; }

    [Networked] protected int CurrentState { get; set; }

    [Networked] private float StartPositionY { get; set; }

    [Networked] private float FlyHeight { get; set; }

    [Networked] private CustomTickTimer FlyTimer { get; set; }


    // Local variables

    [SerializeField] float colliderRadius = .15f;
    [SerializeField] float colliderLength = .5f;

    [SerializeField] float colliderHeight = .7f;

    [SerializeField] AnimationCurve yMotionCurve;

    private float groundDistanceOnThrow;

    protected enum State
    {
        Grounded,
        Flying,
        Inactive
    }



    // Network methods

    public virtual void Initialize(Vector3 direction, float speed, float flyTime)
    {
        CurrentState = speed > 0 ? (int)State.Flying : (int)State.Grounded;
    }


    public override void FixedUpdateNetwork()
    {
        if (CurrentState == (int)State.Flying)
        {
            MoveAndCollide();
        }
    }


    protected virtual void MoveAndCollide()
    {

        transform.position += Velocity * Runner.DeltaTime;

        var normalizedVelocity = Velocity.normalized;

        if (Runner.GetPhysicsScene().SphereCast(transform.position - transform.forward * (colliderLength * .5f),
            colliderRadius, normalizedVelocity, out var hitInfo, colliderLength, LayerMask.GetMask("Wall")))
        {
            Velocity = Vector3.Reflect(Velocity, hitInfo.GetFaceNormal(transform.forward));
            transform.rotation = Quaternion.LookRotation(Velocity);
        }

        float yPosition = EvaluateThrowHeight(FlyTimer.NormalizedValue(Runner));

        //float yDelta = transform.position.y - yPosition;

        transform.position = new(transform.position.x, yPosition, transform.position.z);

        if (FlyTimer.Expired(Runner))
        {
            Velocity = Vector3.zero;
            FlyTimer = CustomTickTimer.None;
            CurrentState = (int)State.Grounded;
        }
    }


    protected virtual float EvaluateThrowHeight(float time)
    {
        float yScalar;
        float yPosition;

        if (time < .5f)
        {
            yScalar = Easings.EaseOutCubic(time * 2) / 2;
            yPosition = StartPositionY + FlyHeight * yScalar;
        }
        else
        {
            yScalar = .5f - Easings.EaseInCubic((time - .5f) * 2) / 2;
            yPosition = (StartPositionY - groundDistanceOnThrow)
                + (FlyHeight + groundDistanceOnThrow) * yScalar;
        }

        return yPosition;
    }


    public virtual void Throw(Vector3 direction, float speed, float flyTime, float flyHeight)
    {
        CurrentState = (int)State.Flying;

        StartPositionY = transform.position.y;
        FlyHeight = flyHeight;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);

        groundDistanceOnThrow = 0;
        if (Runner.GetPhysicsScene().Raycast(transform.position, -transform.up, out var hitInfo, colliderHeight,
            LayerMask.GetMask("Wall")))
        {
            groundDistanceOnThrow = hitInfo.distance - colliderHeight * .5f;
        }

        Velocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction);
    }


    public virtual bool IsPickable()
    {
        return CurrentState == (int)State.Grounded;
    }


    public virtual void OnPickup()
    {
        CurrentState = (int)State.Inactive;
    }



    // Local methods

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }


    public virtual void DrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position - .5f * colliderLength * transform.forward, colliderRadius);
        Gizmos.DrawWireSphere(transform.position + .5f * colliderLength * transform.forward, colliderRadius);
        Gizmos.DrawLine(transform.position - transform.up * -colliderHeight / 2, transform.position + transform.up * -colliderHeight / 2);

        Gizmos.color = Color.white;

        GUIStyle style = new();
        style.normal.textColor = Color.cyan;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString(), style);
    }
}
