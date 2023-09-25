using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInputProvider : MonoBehaviour
{
    [SerializeField] LocalClientInput clientInput;

    PlayerInput playerInput;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        clientInput.SetLocalInput(playerInput);
    }
}
