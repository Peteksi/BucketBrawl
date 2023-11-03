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

    readonly int hitboxLayerMask = 1 << 8;


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
                itemSpawner.Spawn(transform.forward, 20, .75f);
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
            itemQueryHits, layerMask: hitboxLayerMask);

        for (int i = 0; i < count; i++)
        {
            var other = itemQueryHits[i].GameObject;

            if (other != null && other.TryGetComponent(out ItemBase itemBase))
            {
                var directionToOther = other.transform.position - transform.position;

                return itemBase;
            }
        }

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
        Gizmos.color = Color.white;

        Handles.Label(transform.position, ((State)CurrentState).ToString());
    }
}