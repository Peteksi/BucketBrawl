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

    [Range(0, 1)] [SerializeField] float bucketedSpeedMultiplier;

    [SerializeField] float itemPickupRadius;
    [SerializeField] Vector3 itemPickupRadiusOffset;

    [SerializeField] Transform itemHoldTransform;
    [SerializeField] Transform itemWearTransform;

    CustomCharacterController characterController;

    List<LagCompensatedHit> itemQueryHits = new();

    readonly int itemLayerMask = 1 << 8;

    //NetworkRigidbody3D HeldItemRB
    //{
    //    get
    //    {
    //        if (HeldItem.TryGetComponent<NetworkRigidbody3D>(out var rb)) { return rb; }
    //        else { return null; }
    //    }
    //}


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

            var speedMultiplier = 1f;
            if (CurrentState == (int)State.Bucketed) speedMultiplier = bucketedSpeedMultiplier;

            characterController.Move(Runner.DeltaTime * direction, speedMultiplier);


            // collecting & throwing buckets

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
                    HeldItem.transform.position = itemHoldTransform.position;
                    HeldItem.Throw(transform.forward, 20, 1.5f, .75f);

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


    public void EquipItem(ItemBase item)
    {
        CurrentState = (int)State.Bucketed;
        WornItem = item;
    }


    public void UnequipItem()
    {
        CurrentState = (int)State.Default;
        WornItem.Throw(Vector3.zero, 0, 4, 1);
    }


    private ItemBase ItemQuery()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var queryPosition = transform.position;
        queryPosition.y += itemPickupRadiusOffset.y;

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

        Gizmos.color = Color.white;

        GUIStyle style = new();
        style.normal.textColor = Color.yellow;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid)
            Handles.Label(transform.position, ((State)CurrentState).ToString(), style);
    }
}