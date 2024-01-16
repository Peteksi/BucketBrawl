using Fusion;
using UnityEngine;

public struct CustomTickTimer : INetworkStruct
{
    public static CustomTickTimer None => default;

    public readonly bool Expired(NetworkRunner runner) => runner.IsRunning && _target > 0
      && (Tick)_target <= runner.Tick;

    public readonly bool IsRunning => _target > 0;

    private int _target;
    private int _initialTick;


    public static CustomTickTimer CreateFromTicks(NetworkRunner runner, int ticks)
    {
        if (runner == false || runner.IsRunning == false)
            return new CustomTickTimer();

        CustomTickTimer fromTicks = new()
        {
            _target = (int)runner.Tick + ticks,
            _initialTick = runner.Tick
        };

        return fromTicks;
    }


    public static CustomTickTimer CreateFromSeconds(NetworkRunner runner, float seconds)
    {
        if (runner == false || runner.IsRunning == false)
            return new CustomTickTimer();

        return CreateFromTicks(runner, Mathf.RoundToInt(seconds / runner.DeltaTime));
    }


    public float NormalizedValue(NetworkRunner runner)
    {
        if (runner == null || runner.IsRunning == false || IsRunning == false) return 0;

        if (Expired(runner)) return 1;

        return ElapsedTicks(runner) / (_target - (float)_initialTick);
    }


    public int ElapsedTicks(NetworkRunner runner)
    {
        if (!IsRunnerAndTimerRunning(runner)) return 0;

        if (Expired(runner)) return _target - _initialTick; // max value

        return runner.Tick - _initialTick;
    }


    public float ElapsedSeconds(NetworkRunner runner)
    {
        if (!IsRunnerAndTimerRunning(runner)) return 0;

        if (Expired(runner)) return (_target - _initialTick) * runner.DeltaTime; // max value

        return (runner.Tick - _initialTick) * runner.DeltaTime;
    }


    public int RemainingTicks(NetworkRunner runner)
    {
        if (!IsRunnerAndTimerRunning(runner) || Expired(runner)) return 0;

        return _target - runner.Tick;
    }


    public float RemainingSeconds(NetworkRunner runner)
    {
        if (!IsRunnerAndTimerRunning(runner) || Expired(runner)) return 0;

        return (_target - runner.Tick) * runner.DeltaTime;
    }


    private bool IsRunnerAndTimerRunning(NetworkRunner runner)
    {
        // if you don't eat your meat, you can't have any pudding!
        // how can you have any pudding if you don't eat your meat?
        if (runner == false || runner.IsRunning == false || IsRunning == false)
        {
            return false;
        }

        return true;
    }
}