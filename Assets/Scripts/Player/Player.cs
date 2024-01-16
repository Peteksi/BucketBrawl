using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BucketBrawl;
using UnityEngine.UIElements;
using Fusion.Addons.Physics;


[SelectionBase]
public class Player : NetworkBehaviour, IBucketable
{

    // Networked variables

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [Networked] private ItemBase HeldItem { get; set; }

    [Networked] private ItemBase WornItem { get; set; }

    [Networked] private CustomTickTimer UnequipTimer { get; set; }

    [Networked] private int CurrentState { get; set; }


    // Local variables

    [SerializeField] int localPlayerIndex;

    [Header("GAMEPLAY:")]

    [Range(0, 1)] [SerializeField] float bucketedSpeedMultiplier;
    [SerializeField] float unequipTimerSkip;

    [Header("ITEM THROW PARAMETERS:")]

    [SerializeField] ItemBase.FlightParams throwFlightParams;
    [SerializeField] ItemBase.FlightParams unequipFlightParams;
    [SerializeField] ItemBase.FlightParams dropFlightParams;

    [Header("QUERY AREAS:")]

    [SerializeField] float itemPickupRadius;
    [SerializeField] Vector3 itemPickupRadiusOffset;

    [SerializeField] float playerPushRadius;
    [SerializeField] Vector3 playerPushRadiusOffset;

    [Header("REFERENCES:")]

    [SerializeField] Transform itemHoldTransform;
    [SerializeField] Transform itemWearTransform;

    CustomCharacterController characterController;

    List<LagCompensatedHit> itemQueryHits = new();
    List<LagCompensatedHit> pushQueryHits = new();

    readonly int itemLayerMask = 1 << 8;
    readonly int playerLayerMask = 1 << 6;


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


            // Movement

            var direction = new Vector3(inputData.Direction.x, 0, inputData.Direction.y);
            direction.Normalize();

            var speedMultiplier = 1f;
            if (CurrentState == (int)State.Bucketed) speedMultiplier = bucketedSpeedMultiplier;

            characterController.Move(direction * Runner.DeltaTime, speedMultiplier);


            // Handle input

            if (pressedButtons.IsSet(NetworkInputButtons.Action))
            {
                // Default -> Query for items
                if (CurrentState == (int)State.Default)
                {
                    HeldItem = ItemQuery();
                    if (HeldItem != null)
                    {
                        CurrentState = (int)State.HoldingItem;
                        HeldItem.OnPickup();
                    }
                }

                // Holding item -> Throw it
                else if (CurrentState == (int)State.HoldingItem)
                {
                    CurrentState = (int)State.Default;

                    ThrowItem(HeldItem, itemHoldTransform.position, transform.forward, throwFlightParams);
                    HeldItem = null;
                }

                // Bucketed -> Shorten timer
                else if (CurrentState == (int)State.Bucketed)
                {
                    float remainingTime = UnequipTimer.RemainingSeconds(Runner) - unequipTimerSkip;

                    if (remainingTime > 0)
                    {
                        UnequipTimer = CustomTickTimer.CreateFromSeconds(Runner, remainingTime);
                    }
                }
            }
        }


        if (UnequipTimer.Expired(Runner))
        {
            UnequipItem();
            UnequipTimer = CustomTickTimer.None;
        }


        if (CurrentState != (int)State.Bucketed)
        {
            // Query for players to push

            var inputAuthority = Object.InputAuthority;
            var hitboxManager = Runner.LagCompensation;

            var queryPosition = transform.position += playerPushRadiusOffset;

            var count = hitboxManager.OverlapSphere(queryPosition, playerPushRadius, inputAuthority,
                pushQueryHits, layerMask: itemLayerMask);

            for (int i = 0; i < count; i++)
            {
                var other = pushQueryHits[i].GameObject;

                if (other != null && other.TryGetComponent(out Player player))
                {

                }
            }
        }
    }


    public bool IsBucketable()
    {
        return CurrentState != (int)State.Bucketed;
    }


    public void EquipItem(ItemBase item, float duration)
    {
        CurrentState = (int)State.Bucketed;
        WornItem = item;
        if (duration > 0) UnequipTimer = CustomTickTimer.CreateFromSeconds(Runner, duration);

        if (HeldItem)
        {
            ThrowItem(HeldItem, itemHoldTransform.position, transform.forward, dropFlightParams);
            HeldItem = null;
        }
    }


    public void UnequipItem()
    {
        CurrentState = (int)State.Default;

        ThrowItem(WornItem, itemWearTransform.position, -transform.forward, unequipFlightParams);
        WornItem = null;
    }


    public void ThrowItem(ItemBase item, Vector3 from, Vector3 direction, ItemBase.FlightParams flightParams)
    {
        item.transform.position = from;
        item.Throw(direction, flightParams);
    }


    private ItemBase ItemQuery()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var queryPosition = transform.position += itemPickupRadiusOffset;

        var count = hitboxManager.OverlapSphere(queryPosition, itemPickupRadius, inputAuthority,
            itemQueryHits, layerMask: itemLayerMask);


        // choose the item most directly in front of the player

        var largestDot = -2f;
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
        characterController = GetComponent<CustomCharacterController>();
    }


    public void LateUpdate()
    {
        if (HeldItem != null)
        {
            HeldItem.transform.SetPositionAndRotation(itemHoldTransform.position, itemHoldTransform.rotation);
        }

        if (WornItem != null)
        {
            WornItem.transform.SetPositionAndRotation(itemWearTransform.position, itemWearTransform.rotation);
        }
    }


    private void OnDrawGizmos()
    {
        var queryPosition = transform.position + itemPickupRadiusOffset;

        Gizmos.color = new(1, 1, 1, .1f);
        CustomGizmos.DrawCircle(queryPosition, transform.up, itemPickupRadius);

        if (EditorApplication.isPlaying && Object != null && Object.IsValid && HeldItem != null)
            Gizmos.DrawLine(transform.position, HeldItem.transform.position);

        if (EditorApplication.isPlaying && Object != null && Object.IsValid && WornItem != null)
            Gizmos.DrawLine(transform.position, WornItem.transform.position);

        Gizmos.color = Color.white;

        GUIStyle style = new();
        style.normal.textColor = Color.yellow;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid)
            Handles.Label(transform.position, ((State)CurrentState).ToString(), style);
    }
}