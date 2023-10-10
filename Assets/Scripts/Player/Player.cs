using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomCharacterController), typeof(ItemSpawner))]
public class Player : NetworkBehaviour
{
    [SerializeField] private int playerIndex;

    private CustomCharacterController characterController;
    private ItemSpawner itemSpawner;

    [Networked] public NetworkButtons PreviousButtons { get; set; }


    private void Awake()
    {
       characterController = GetComponent<CustomCharacterController>();
       itemSpawner = GetComponent<ItemSpawner>();
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData combinedInputData))
        {
            var inputData = combinedInputData[playerIndex];

            //var pressedButtons = inputData.

            // movement

            var direction = new Vector3(inputData.Direction.x, 0, inputData.Direction.y);
            direction.Normalize();

            characterController.Move(Runner.DeltaTime * direction);


            // throwing buckets

            if (inputData.Buttons.IsSet(NetworkInputButtons.Action))
            {
                itemSpawner.Spawn();
            }
        }
    }
}