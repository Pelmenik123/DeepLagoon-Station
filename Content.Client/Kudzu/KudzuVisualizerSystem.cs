using Content.Shared.Spreader;
using Robust.Client.GameObjects;

namespace Content.Client.Kudzu;

public sealed partial class KudzuVisualsSystem : VisualizerSystem<KudzuVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, KudzuVisualsComponent component, ref AppearanceChangeEvent args)
    {

        if (args.Sprite == null)
            return;
        if (AppearanceSystem.TryGetData<int>(uid, KudzuVisuals.Variant, out var var, args.Component)
            && AppearanceSystem.TryGetData<int>(uid, KudzuVisuals.GrowthLevel, out var level, args.Component))
        {
            var index = _sprite.LayerMapReserve(args.Sprite.AsEntity(), component.Layer.ToString());
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), index, $"kudzu_{level}{var}");
        }
    }
}
