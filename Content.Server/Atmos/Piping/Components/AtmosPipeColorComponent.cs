using Content.Server.Atmos.Piping.EntitySystems;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Piping.Components;

[RegisterComponent]
public sealed partial class AtmosPipeColorComponent : Component
{
    [DataField]
    public Color Color { get; set; } = Color.White;

    [ViewVariables(VVAccess.ReadWrite), UsedImplicitly]
    public Color ColorVV
    {
        get => Color;
#pragma warning disable CS0618 // Matches upstream SS14: VV-only setter has no EntityUid parameter.
        set => IoCManager.Resolve<IEntityManager>().System<AtmosPipeColorSystem>().SetColor(Owner, this, value);
#pragma warning restore CS0618
    }
}

[ByRefEvent]
public record struct AtmosPipeColorChangedEvent(Color color)
{
    public Color Color = color;
}
