using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Engineering;

/// <summary>
/// Raised when the do-after for the spawn-after-interact component completes.
/// Lives in shared so it can be net-serialized as part of the do-after state.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SpawnAfterInteractDoAfterEvent : DoAfterEvent
{
    [DataField("location")]
    public NetCoordinates ClickLocation;

    public SpawnAfterInteractDoAfterEvent(IEntityManager entManager, EntityCoordinates clickLocation)
    {
        ClickLocation = entManager.GetNetCoordinates(clickLocation);
    }

    private SpawnAfterInteractDoAfterEvent()
    {
    }

    public override DoAfterEvent Clone()
    {
        return this;
    }
}
