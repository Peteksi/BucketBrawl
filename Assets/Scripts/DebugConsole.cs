using UnityEngine;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    private string commandInput = "";
    private Vector2 consoleScrollPosition = Vector2.zero;
    private List<string> consoleLog = new List<string>();
    private GUIStyle consoleTextStyle;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Start()
    {
        consoleTextStyle = new GUIStyle(GUI.skin.textArea);
        consoleTextStyle.wordWrap = true;
    }

    private void Update()
    {
        // Toggle the console with a key press (you can customize this)
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleConsole();
        }
    }

    private void OnGUI()
    {
        if (!enabled)
            return;

        DrawConsoleWindow();
    }

    private void DrawConsoleWindow()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, Screen.width - 20, 190), "Debug Console", "Window");

        // Display the console log
        consoleScrollPosition = GUILayout.BeginScrollView(consoleScrollPosition, GUILayout.ExpandHeight(true));
        GUILayout.TextArea(string.Join("\n", consoleLog.ToArray()), consoleTextStyle, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        // Input field for adding commands
        GUILayout.BeginHorizontal();
        commandInput = GUILayout.TextField(commandInput, GUILayout.ExpandWidth(true));

        // Process command input when Enter is pressed
        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && commandInput != "")
        {
            ExecuteCommand(commandInput);
            commandInput = "";
            Event.current.Use();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void ToggleConsole()
    {
        // Toggle the visibility of the console
        enabled = !enabled;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Handle Unity log messages and add them to the console log
        string logEntry = $"{type.ToString()}: {logString}";
        consoleLog.Add(logEntry);

        // Trim the console log to a certain number of lines to prevent excessive memory usage
        const int maxLogLines = 100;
        if (consoleLog.Count > maxLogLines)
        {
            consoleLog.RemoveAt(0);
        }
    }

    private void ExecuteCommand(string command)
    {
        // Add your command execution logic here
        // For now, we'll just add the command to the console log
        consoleLog.Add($"Command: {command}");
    }
}
