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
        var networkPlayer = new NetworkInputPlayer();

        var direction = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        var actionPressed = playerInput.actions.FindAction("Action").IsPressed();

        networkPlayer.Direction = direction;
        if (actionPressed) networkPlayer.Buttons.Set(NetworkInputButtons.Action, true);

        networkInput[playerInput.playerIndex] = networkPlayer;
        ClientInput = networkInput;
    }
}