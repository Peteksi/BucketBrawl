using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private int playerIndex;

    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
       characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData inputData))
        {
            var direction = new Vector3(inputData[playerIndex].Direction.x, 0, inputData[playerIndex].Direction.y);
            direction.Normalize();

            characterController.Move(Runner.DeltaTime * direction);
        }
    }
}