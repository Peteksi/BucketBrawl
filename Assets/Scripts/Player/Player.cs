using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomCharacterController), typeof(ItemSpawner))]
public class Player : NetworkBehaviour
{
    [SerializeField] private int playerIndex;

    [Networked] public NetworkButtons PreviousButtons { get; set; }

    private CustomCharacterController characterController;
    private ItemSpawner itemSpawner;


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

            var pressedButtons = inputData.Buttons.GetPressed(PreviousButtons);

            PreviousButtons = inputData.Buttons;

            // movement

            var direction = new Vector3(inputData.Direction.x, 0, inputData.Direction.y);
            direction.Normalize();

            characterController.Move(Runner.DeltaTime * direction);


            // throwing buckets

            if (pressedButtons.IsSet(NetworkInputButtons.Action))
            {
                itemSpawner.Spawn(transform.forward, 12f);
            } 
        }
    }
}