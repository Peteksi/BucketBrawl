using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEditor;

public class Bucket : ItemBase
{

    // Networked variables

    [HideInInspector] [Networked] public Vector3 Velocity { get; set; }

    [Networked] private int CurrentState { get; set; }

    [Networked] private float StartPositionY { get; set; }

    [Networked] private CustomTickTimer FlyTimer { get; set; }


    // Local variables

    [SerializeField] float colliderRadius;
    [SerializeField] float colliderLength;

    [SerializeField] float hitBoxRadius;

    [SerializeField] AnimationCurve yMotionCurve;

    List<LagCompensatedHit> hits = new();

    readonly int playerLayerMask = 1 << 6;

    enum State
    {
        Grounded,
        Flying,
        Held
    }



    // Network methods

    public override void Initialize(Vector3 direction, float speed, float flyTime)
    {
        
    }


    public override void FixedUpdateNetwork()
    {
        if (CurrentState == (int)State.Flying)
        {
            MoveAndCollide();
            HitboxQuery();
        }

        if (!Object || !Object.IsValid) return;

        if (FlyTimer.Expired(Runner))
        {
            CurrentState = (int)State.Grounded;
            Velocity = Vector3.zero;
            FlyTimer = CustomTickTimer.None;
        };
    }


    private void MoveAndCollide()
    {
        transform.position += Velocity * Runner.DeltaTime;

        var normalizedVelocity = Velocity.normalized;

        if (Runner.GetPhysicsScene().SphereCast(transform.position - transform.forward * (colliderLength * .5f),
            colliderRadius, normalizedVelocity, out var hitInfo, colliderLength, LayerMask.GetMask("Wall")))
        {
            Velocity = Vector3.Reflect(Velocity, hitInfo.GetFaceNormal(transform.forward));
            transform.rotation = Quaternion.LookRotation(Velocity);
        }

        transform.position = new(
            transform.position.x,
            StartPositionY + yMotionCurve.Evaluate(FlyTimer.NormalizedValue(Runner)),
            transform.position.z
        );
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
                    bucketable.EquipBucket();
                    Runner.Despawn(Object);
                    break;
                }
            }
        }
    }


    public override bool IsPickable()
    {
        return CurrentState == (int)State.Grounded;
    }


    public override void OnPickup()
    {
        CurrentState = (int)State.Held;
    }


    public override void Throw(Vector3 direction, float speed, float flyTime)
    {
        CurrentState = (int)State.Flying;
        StartPositionY = transform.position.y;
        FlyTimer = CustomTickTimer.CreateFromSeconds(Runner, flyTime);

        Velocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction);
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

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString());

    }


    //public override void Render()
    //{
    //    transform.Rotate(200 * Time.deltaTime, 0, 0);
    //}
}