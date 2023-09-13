using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProvider : MonoBehaviour
{
    PlayerControls _playerControls = new();

    private void OnEnable()
    {
        _playerControls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Gameplay.Disable();
    }

    private void Update()
    {
        //if ()
    }
}
