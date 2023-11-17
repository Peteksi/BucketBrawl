using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//[RequireComponent(typeof(CustomCharacterController), typeof(ItemSpawner))]
public class Player : NetworkBehaviour, IBucketable
{

    // Networked variables

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [Networked] private ItemBase HeldItem { get; set; }

    [Networked] private ItemBase WornItem { get; set; }

    [Networked] private int CurrentState { get; set; }


    // Local variables

    [SerializeField] int localPlayerIndex;

    [SerializeField] float itemPickupRadius;
    [SerializeField] float itemPickupRadiusYOffset;

    [SerializeField] float itemHoldOffset;
    [SerializeField] float itemWearOffset;

    NetworkCharacterController characterController;

    List<LagCompensatedHit> itemQueryHits = new();

    readonly int itemLayerMask = 1 << 8;


    enum State
    {
        Default,
        Bucketed,
        HoldingItem
    }



    // Network methods

    public override void Spawned()
    {
        CurrentState = (int)State.Default;
    }
    

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData combinedInputData))
        {
            var inputData = combinedInputData[localPlayerIndex];
            var pressedButtons = inputData.Buttons.GetPressed(PreviousButtons);
            PreviousButtons = inputData.Buttons;


            // movement

            var direction = new Vector3(inputData.Direction.x, 0, inputData.Direction.y);
            direction.Normalize();

            characterController.Move(Runner.DeltaTime * direction);


            // throwing buckets

            if (pressedButtons.IsSet(NetworkInputButtons.Action))
            {
                if (CurrentState == (int)State.Default)
                {
                    HeldItem = ItemQuery();
                    if (HeldItem != null)
                    {
                        CurrentState = (int)State.HoldingItem;
                        HeldItem.OnPickup();
                    }
                }
                else if (CurrentState == (int)State.HoldingItem)
                {
                    HeldItem.transform.position = transform.position + transform.forward * itemHoldOffset;
                    HeldItem.Throw(transform.forward, 20, .75f);

                    CurrentState = (int)State.Default;
                    HeldItem = null;
                }
            } 
        }
    }


    public bool IsBucketable()
    {
        return CurrentState != (int)State.Bucketed;
    }


    public void EquipBucket(ItemBase item)
    {
        CurrentState = (int)State.Bucketed;
        WornItem = item;
    }


    private ItemBase ItemQuery()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var queryPosition = transform.position;
        queryPosition.y += itemPickupRadiusYOffset;

        var count = hitboxManager.OverlapSphere(queryPosition, itemPickupRadius, inputAuthority,
            itemQueryHits, layerMask: itemLayerMask);


        // choose the item most directly in front of the player

        var largestDot = -1f;
        ItemBase largestDotItem = null;

        for (int i = 0; i < count; i++)
        {
            var other = itemQueryHits[i].GameObject;

            if (other != null && other.TryGetComponent(out ItemBase itemBase) && itemBase.IsPickable())
            {
                var directionToOther = other.transform.position - transform.position;
                var dotToOther = Vector3.Dot(directionToOther.normalized, transform.forward);

                if (dotToOther >= largestDot)
                {
                    largestDot = dotToOther;
                    largestDotItem = itemBase;
                }
            }
        }

        if (largestDotItem != null)
        {
            return largestDotItem;
        }

        return null;
    }
    


    // Local methods

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterController>();
    }


    public void LateUpdate()
    {
        if (HeldItem != null)
        {
            HeldItem.transform.SetPositionAndRotation(
                transform.position + transform.forward * itemHoldOffset,
                transform.rotation
            );
        }

        if (WornItem != null)
        {
            var itemRotation = transform.rotation * Quaternion.Euler(0, 0, 180);

            WornItem.transform.SetPositionAndRotation(
                transform.position + transform.up * itemWearOffset,
                itemRotation
            );
        }
    }


    private void OnDrawGizmos()
    {
        var queryPosition = transform.position;
        queryPosition.y += itemPickupRadiusYOffset;

        Gizmos.color = new(1, 1, 1, .1f);
        CustomGizmos.DrawGizmoCircle(queryPosition, transform.up, itemPickupRadius);

        if (EditorApplication.isPlaying && Object != null && Object.IsValid && HeldItem != null) Gizmos.DrawLine(transform.position, HeldItem.transform.position);
        Gizmos.color = Color.white;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString());
    }
}