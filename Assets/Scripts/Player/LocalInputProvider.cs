using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class LocalInputProvider : MonoBehaviour
{
    [SerializeField] LocalClientInput clientInput;

    PlayerInput playerInput;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        clientInput.SetLocalInput(playerInput);
    }
}
