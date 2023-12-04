using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BucketBrawl;
using Fusion.Addons.Physics;


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

    private float groundDistanceOnThrow;

    private ChangeDetector changes;

    readonly int wallLayerMask = 1 << 7;

    NetworkRigidbody3D RigidBody
    {
        get
        {
            if (this.TryGetComponent<NetworkRigidbody3D>(out var rb)) { return rb; }
            else { return null; }
        }
    }

    protected enum State
    {
        Default,
        Flying,
        Inactive
    }



    // Network methods

    public virtual void Initialize(Vector3 direction, float speed, float flyHeight, float flyTime)
    {
        CurrentState = speed > 0 ? (int)State.Flying : (int)State.Default;
    }


    public override void Spawned()
    {
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }


    public override void FixedUpdateNetwork()
    {
        if (CurrentState == (int)State.Flying)
        {
            MoveAndCollide();
        }

        HandleChangeDetection();
    }


    private void OnStateChanged(int previous, int current)
    {
        if (previous == current) return;

        if (current == (int)State.Default)
        {
            if (RigidBody != null) RigidBody.RBIsKinematic = false;
        }

        if (previous == (int)State.Default)
        {
            if (RigidBody != null) RigidBody.RBIsKinematic = true;
        }
    }


    protected virtual void MoveAndCollide()
    {
        var normalizedVelocity = Velocity.normalized;

        if (Runner.GetPhysicsScene().SphereCast(transform.position - transform.forward * (colliderLength * .5f),
            colliderRadius, normalizedVelocity, out var hitInfo, colliderLength, wallLayerMask))
        {
            var yOld = Velocity.y;
            Velocity = Vector3.Reflect(Velocity, hitInfo.GetFaceNormal(transform.forward));
            Velocity = new Vector3(Velocity.x, yOld, Velocity.z);

            transform.rotation = Quaternion.LookRotation(Velocity);
        }

        float yDelta = 0;

        if (FlyTimer.IsRunning)
        {
            float yPosition = EvaluateThrowHeight(FlyTimer.NormalizedValue(Runner));
            yDelta = yPosition - transform.position.y;

            Velocity = new(Velocity.x, yDelta / Runner.DeltaTime, Velocity.z);
        }

        if (FlyTimer.Expired(Runner))
        {
            //Velocity = yDelta < 0 ? new Vector3(Velocity.x, yDelta / Runner.DeltaTime, Velocity.z) : Velocity;

            FlyTimer = CustomTickTimer.None;
        }

        transform.position += Velocity * Runner.DeltaTime;
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

            float groundHeight = StartPositionY - groundDistanceOnThrow;
            yPosition = groundHeight + (FlyHeight + groundDistanceOnThrow * 2) * yScalar;
        }

        return yPosition;
    }


    public virtual void Throw(Vector3 direction, float speed, float flyHeight, float flyTime)
    {
        CurrentState = (int)State.Flying;

        StartPositionY = transform.position.y;
        FlyHeight = flyHeight;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);

        groundDistanceOnThrow = 0;
        if (Runner.GetPhysicsScene().Raycast(transform.position, -transform.up, out var hitInfo, colliderHeight,
            wallLayerMask))
        {
            groundDistanceOnThrow = hitInfo.distance - colliderHeight * .5f;
        }

        Velocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction);
    }


    public virtual bool IsGrounded(out RaycastHit hitInfo)
    {
        hitInfo = default;

        if (Runner.GetPhysicsScene().SphereCast(transform.position, colliderRadius * .95f, -transform.up,
            out var raycastHit, colliderHeight * .5f, wallLayerMask))
        {
            hitInfo = raycastHit;
        }

        return false;
    }


    public virtual bool IsPickable()
    {
        return CurrentState == (int)State.Default;
    }


    public virtual void OnPickup()
    {
        CurrentState = (int)State.Inactive;
    }


    private void HandleChangeDetection()
    {
        foreach (var change in changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(CurrentState):
                    var reader = GetPropertyReader<int>(nameof(CurrentState));
                    var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                    OnStateChanged(previous, current);
                    break;
            }
        }
    }



    // Local methods

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }


    public virtual void DrawGizmos()
    {
        Gizmos.color = Color.blue;

        CustomGizmos.DrawWireCapsule(transform.position - colliderLength * transform.forward * .5f,
            transform.position + colliderLength * transform.forward * .5f, colliderRadius);

        CustomGizmos.DrawWireCapsule(transform.position,
            transform.position - colliderHeight * transform.up * .5f, colliderRadius * .95f);

        Gizmos.color = Color.white;

        GUIStyle style = new();
        style.normal.textColor = Color.cyan;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString(), style);
    }
}
