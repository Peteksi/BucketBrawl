using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(CustomCharacterController), typeof(ItemSpawner))]
public class Player : NetworkBehaviour, IBucketable
{

    // Networked variables

    [Networked] private NetworkButtons PreviousButtons { get; set; }

    [Networked] private ItemBase HeldItem { get; set; }

    [Networked] private int CurrentState { get; set; }


    // Local variables

    [SerializeField] int localPlayerIndex;

    [SerializeField] float itemPickupRadius;

    CustomCharacterController characterController;
    ItemSpawner itemSpawner;

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
                    if (HeldItem != null) { CurrentState = (int)State.HoldingItem; }
                }
                else
                {
                    HeldItem = null;
                }
            } 
        }
    }


    public bool IsBucketable()
    {
        return CurrentState == (int)State.Default;
    }


    public void EquipBucket()
    {
        CurrentState = (int)State.Bucketed;
    }


    private ItemBase ItemQuery()
    {
        var inputAuthority = Object.InputAuthority;
        var hitboxManager = Runner.LagCompensation;

        var count = hitboxManager.OverlapSphere(transform.position, itemPickupRadius, inputAuthority,
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

        if (largestDotItem != null) return largestDotItem;

        return null;
    }
    


    // Local methods

    private void Awake()
    {
        characterController = GetComponent<CustomCharacterController>();
        itemSpawner = GetComponent<ItemSpawner>();
    }


    private void OnStateChanged()
    {
        
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new(1, 1, 1, .1f);
        CustomGizmos.DrawGizmoCircle(transform.position, transform.up, itemPickupRadius);

        if (EditorApplication.isPlaying && Object != null && Object.IsValid && HeldItem != null) Gizmos.DrawLine(transform.position, HeldItem.transform.position);
        Gizmos.color = Color.white;

        if (EditorApplication.isPlaying && Object != null && Object.IsValid) Handles.Label(transform.position, ((State)CurrentState).ToString());
    }
}