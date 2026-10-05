using Content.Shared.Tools.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Tools.Visualizers;

public sealed partial class WeldableVisualizerSystem : VisualizerSystem<WeldableComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, WeldableComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        AppearanceSystem.TryGetData<bool>(uid, WeldableVisuals.IsWelded, out var isWelded, args.Component);
        if (_sprite.LayerMapTryGet(args.Sprite.AsEntity(), WeldableLayers.BaseWelded, out var layer, false))
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, isWelded);
        }
    }
}
