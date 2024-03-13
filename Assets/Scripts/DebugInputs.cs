using UnityEngine;
using Fusion;

public class DebugInputs : MonoBehaviour
{
    public DebugConsole debugConsole;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (debugConsole != null)
            {
                debugConsole.enabled = !debugConsole.enabled;
            }
            else
            {
                Debug.LogWarning("DebugConsole component is not assigned!");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (debugConsole != null)
            {
                debugConsole.enabled = false;
            }
            else
            {
                Debug.LogWarning("DebugConsole component is not assigned!");
            }
        }
    }
}