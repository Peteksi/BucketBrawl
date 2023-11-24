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

    [SerializeField] float groundRaycastLength = .5f;

    [SerializeField] AnimationCurve yMotionCurve;

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

        float yScalar;

        if (FlyTimer.NormalizedValue(Runner) < .5f) {
            yScalar = Easings.EaseOutCubic(FlyTimer.NormalizedValue(Runner) * 2) / 2;
        }
        else
        {
            yScalar = .5f - Easings.EaseInCubic((FlyTimer.NormalizedValue(Runner) - .5f) * 2) / 2;
        }

        float yPosition = StartPositionY + FlyHeight * yScalar;
        float yDelta = transform.position.y - yPosition;

        transform.position = new(transform.position.x, yPosition, transform.position.z);

        if (FlyTimer.Expired(Runner))
        {
            Velocity = new(Velocity.x, yDelta, Velocity.z);
            FlyTimer = CustomTickTimer.None;
        }
    }


    public virtual void Throw(Vector3 direction, float speed, float flyTime, float flyHeight)
    {
        CurrentState = (int)State.Flying;
        StartPositionY = transform.position.y;
        FlyHeight = flyHeight;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);

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
        Gizmos.DrawLine(transform.position, transform.position + transform.up * -groundRaycastLength);

        Gizmos.color = Color.white;

        GUIStyle style = new();
        style.normal.textColor = Color.cyan;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString(), style);
    }
}
