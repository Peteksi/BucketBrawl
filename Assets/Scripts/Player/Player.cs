using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _characterController;

    private void Awake()
    {
        _characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData inputData))
        {
            var direction = new Vector3(inputData.direction.x, 0, inputData.direction.y);
            direction.Normalize();

            _characterController.Move(Runner.DeltaTime * direction);
        }
    }
}