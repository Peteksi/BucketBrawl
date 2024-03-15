using UnityEngine;
using System.Collections.Generic;
using System;
using Fusion;

public class DebugConsole : MonoBehaviour
{
    public FusionBootstrap bootstrap;

    private string commandInput = "";
    private Vector2 consoleScrollPosition = Vector2.zero;
    private List<string> consoleLog = new List<string>();
    private GUIStyle consoleTextStyle;
    private bool focusTextField = true;

    //private void OnEnable()
    //{
    //    Application.logMessageReceived += HandleLog;
    //}

    //private void OnDisable()
    //{
    //    Application.logMessageReceived -= HandleLog;
    //}

    private void Start()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        focusTextField = true;
    }

    private void OnGUI()
    {
        if (!enabled)
            return;

        if (consoleTextStyle == null)
        {
            consoleTextStyle = new GUIStyle(GUI.skin.textArea);
            consoleTextStyle.wordWrap = true;
        }

        DrawConsoleWindow();
    }

    private void DrawConsoleWindow()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, Screen.width - 20, 190), "Debug", "Window");

        // Display the console log
        consoleScrollPosition = GUILayout.BeginScrollView(consoleScrollPosition, GUILayout.ExpandHeight(true));
        GUILayout.TextArea(string.Join("\n", consoleLog.ToArray()), consoleTextStyle, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        // Input field for adding commands
        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("CommandLine");
        commandInput = GUILayout.TextField(commandInput, GUILayout.ExpandWidth(true));

        if (focusTextField)
        {
            GUI.FocusControl("CommandLine");
            focusTextField = false;
        }

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

    private void HandleLog(string logString, string stackTrace, UnityEngine.LogType type)
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
        consoleLog.Add($">{command}");

        string[] commandArray = command.Split(' ');

        string commandBase = commandArray[0];

        string var1 = commandArray.Length > 1 ? commandArray[1] : null;

        switch (commandBase)
        {
            case "shutdown":
            case "shut":
                bootstrap.ShutdownAll();
                break;

            case "auto":
            case "a":
                if (var1 == null || var1 == "1")
                {
                    bootstrap.StartAutoClient();
                }
                else { bootstrap.StartMultipleAutoClients(Convert.ToInt32(var1)); }

                break;

            case "host":
            case "h":
                bootstrap.StartHost();
                break;

            case "client":
            case "c":
            case "join":
            case "j":
                bootstrap.StartClient();
                break;
        }

        enabled = false;
    }
}
