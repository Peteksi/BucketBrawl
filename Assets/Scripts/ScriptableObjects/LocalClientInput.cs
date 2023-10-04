using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "LocalClientInput", menuName = "ScriptableObjects/LocalClientInput")]
public class LocalClientInput : ScriptableObject
{
    public NetworkInputData ClientInput { get; private set; } = new NetworkInputData();

    public NetworkInputPlayer LocalPlayerOneInput
    {
        get
        {
            return ClientInput[0];
        }
    }


    public void SetLocalInput(PlayerInput playerInput)
    {
        var networkInput = ClientInput;
        var direction = playerInput.actions.FindAction("Move").ReadValue<Vector2>();

        networkInput[playerInput.playerIndex] = new NetworkInputPlayer() { Direction = direction };

        ClientInput = networkInput;
    }
}