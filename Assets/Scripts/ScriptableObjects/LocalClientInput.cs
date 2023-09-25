using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "LocalClientInput", menuName = "ScriptableObjects/LocalClientInput")]
public class LocalClientInput : ScriptableObject
{
    public List<NetworkInputData> LocalPlayerInputs { get; private set; } = new List<NetworkInputData>();

    public int LocalPlayerCount { get { return LocalPlayerInputs.Count; } }


    public NetworkInputData LocalPlayerOneInput
    {
        get
        {
            if (LocalPlayerCount == 0) LocalPlayerInputs.Add(new NetworkInputData());

            return LocalPlayerInputs[0];
        }
    }


    public void SetLocalInput(PlayerInput playerInput)
    {
        var direction = playerInput.actions.FindAction("Move").ReadValue<Vector2>();

        var networkInput = new NetworkInputData();

        networkInput[playerInput.playerIndex] = new NetworkInputPlayer() { Direction = direction };

        if (playerInput.playerIndex >= LocalPlayerCount)
        {
            LocalPlayerInputs.Add(networkInput);
        }
        else
        {
            LocalPlayerInputs[playerInput.playerIndex] = networkInput;
        }
    }
}