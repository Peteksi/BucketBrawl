using Fusion;
using static Fusion.NetworkBehaviour;

public abstract class NetworkBehaviourWithState<T> : NetworkBehaviour where T : unmanaged, INetworkStruct
{
    public abstract ref T State { get; }

    private ChangeDetector _changesSimulation;
    private ChangeDetector _changesFrom;
    private ChangeDetector _changesTo;


    protected bool TryGetStateChanges(out T previous, out T current, ChangeDetector.Source source = ChangeDetector.Source.SimulationState)
    {
        switch (source)
        {
            default:
            case ChangeDetector.Source.SimulationState:
                return TryGetStateChanges(source, ref _changesSimulation, out previous, out current);
            case ChangeDetector.Source.SnapshotFrom:
                return TryGetStateChanges(source, ref _changesFrom, out previous, out current);
            case ChangeDetector.Source.SnapshotTo:
                return TryGetStateChanges(source, ref _changesTo, out previous, out current);
        }
    }


    private bool TryGetStateChanges(ChangeDetector.Source source, ref ChangeDetector changes, out T previous, out T current)
    {
        changes ??= GetChangeDetector(source);

        if (changes != null)
        {
            foreach (var change in changes.DetectChanges(this, out var previousBuffer, out var currentBuffer))
            {
                switch (change)
                {
                    case nameof(State):
                        var reader = GetPropertyReader<T>(change);
                        (previous, current) = reader.Read(previousBuffer, currentBuffer);
                        return true;
                }
            }
        }

        current = default;
        previous = default;
        return false;
    }


    protected bool TryGetStateSnapshots(out T from, out Tick fromTick, out T to, out Tick toTick, out float alpha)
    {
        if (TryGetSnapshotsBuffers(out var fromBuffer, out var toBuffer, out alpha))
        {
            var reader = GetPropertyReader<T>(nameof(State));
            (from, to) = reader.Read(fromBuffer, toBuffer);
            fromTick = fromBuffer.Tick;
            toTick = toBuffer.Tick;
            return true;
        }

        from = default;
        to = default;
        fromTick = default;
        toTick = default;
        return false;
    }
}