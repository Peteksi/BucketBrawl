using Fusion;

public struct CustomTickTimer : INetworkStruct
{
    private int _target;
    private int _initialTick;

    public bool Expired(NetworkRunner runner) => runner.IsRunning && _target > 0
      && (Tick)_target <= runner.Simulation.Tick;

    public bool IsRunning => _target > 0;


    public static CustomTickTimer CreateFromTicks(NetworkRunner runner, int ticks)
    {
        if (runner == false || runner.IsRunning == false)
            return new CustomTickTimer();

        CustomTickTimer fromTicks = new CustomTickTimer();
        fromTicks._target = (int)runner.Simulation.Tick + ticks;
        fromTicks._initialTick = runner.Simulation.Tick;
        return fromTicks;
    }


    public float NormalizedValue(NetworkRunner runner)
    {
        if (runner == null || runner.IsRunning == false || IsRunning == false)
            return 0;

        if (Expired(runner))
            return 1;

        return ElapsedTicks(runner) / (_target - (float)_initialTick);
    }


    public int ElapsedTicks(NetworkRunner runner)
    {
        if (runner == false || runner.IsRunning == false)
            return 0;

        if (IsRunning == false || Expired(runner))
            return 0;

        return runner.Simulation.Tick - _initialTick;
    }
}