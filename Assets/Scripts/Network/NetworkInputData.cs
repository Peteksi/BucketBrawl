using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct NetworkInputData : INetworkInput
{
    private NetworkInputPlayer playerA;
    private NetworkInputPlayer playerB;
    private NetworkInputPlayer playerC;
    private NetworkInputPlayer playerD;

    public NetworkInputPlayer this[int i]
    {
        get
        {
            switch (i)
            {
                case 0: return playerA;
                case 1: return playerB;
                case 2: return playerC;
                case 3: return playerD;
                default: return default;
            }
        }

        set
        {
            switch (i)
            {
                case 0: playerA = value; return;
                case 1: playerB = value; return;
                case 2: playerC = value; return;
                case 3: playerD = value; return;
                default: return;
            }
        }
    }
}


public struct NetworkInputPlayer : INetworkStruct
{
    public Vector2 Direction;
}