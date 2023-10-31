using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomCharacterController), typeof(ItemSpawner))]
public class Player : NetworkBehaviour, IBucketable
{
    [Networked] public NetworkButtons PreviousButtons { get; set; }

    [Networked] private int CurrentState { get; set; }



    [SerializeField] int localPlayerIndex;

    CustomCharacterController characterController;
    ItemSpawner itemSpawner;

    enum State
    {
        Default = 0,
        Bucketed = 1,
    }





    #region Networked

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
    #endregion





    #region Local

    private void Awake()
    {
        characterController = GetComponent<CustomCharacterController>();
        itemSpawner = GetComponent<ItemSpawner>();
    }

    private void OnBucketedChanged()
    {

    }
    #endregion
}