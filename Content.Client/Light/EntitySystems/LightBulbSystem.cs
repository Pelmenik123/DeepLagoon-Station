using Content.Shared.Light.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Light.Visualizers;

public sealed partial class LightBulbSystem : VisualizerSystem<LightBulbComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, LightBulbComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // update sprite state
        if (AppearanceSystem.TryGetData<LightBulbState>(uid, LightBulbVisuals.State, out var state, args.Component))
        {
            switch (state)
            {
                case LightBulbState.Normal:
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), LightBulbVisualLayers.Base, comp.NormalSpriteState);
                    break;
                case LightBulbState.Broken:
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), LightBulbVisualLayers.Base, comp.BrokenSpriteState);
                    break;
                case LightBulbState.Burned:
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), LightBulbVisualLayers.Base, comp.BurnedSpriteState);
                    break;
            }
        }

        // also update sprites color
        if (AppearanceSystem.TryGetData<Color>(uid, LightBulbVisuals.Color, out var color, args.Component))
        {
            _sprite.SetColor(args.Sprite.AsEntity(), color);
        }
    }
}
