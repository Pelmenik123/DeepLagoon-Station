using Robust.Client.GameObjects;
using Content.Shared.Smoking;

namespace Content.Client.Smoking;

public sealed partial class BurnStateVisualizerSystem : VisualizerSystem<BurnStateVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, BurnStateVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (!args.AppearanceData.TryGetValue(SmokingVisuals.Smoking, out var burnState))
            return;

        var state = burnState switch
        {
            SmokableState.Lit => component.LitIcon,
            SmokableState.Burnt => component.BurntIcon,
            _ => component.UnlitIcon
        };

        _sprite.LayerSetRsiState(args.Sprite.AsEntity(), 0, state);
    }
}

