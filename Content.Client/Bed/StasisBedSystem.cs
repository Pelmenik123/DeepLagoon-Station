using Content.Shared.Bed;
using Robust.Client.GameObjects;

namespace Content.Client.Bed;

public sealed partial class StasisBedSystem : VisualizerSystem<StasisBedVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, StasisBedVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite != null
            && AppearanceSystem.TryGetData<bool>(uid, StasisBedVisuals.IsOn, out var isOn, args.Component))
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), StasisBedVisualLayers.IsOn, isOn);
        }
    }
}

public enum StasisBedVisualLayers : byte
{
    IsOn,
}
