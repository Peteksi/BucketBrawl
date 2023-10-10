using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

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
        var action = playerInput.actions.FindAction("Action");

        bool actionPressed = action.IsPressed();

        networkInput[playerInput.playerIndex] = new NetworkInputPlayer()
        {
            Direction = direction,
            Buttons = { Action = (NetworkBool)actionPressed }
        };

        networkInput[playerInput.playerIndex]

        ClientInput = networkInput;
    }
}