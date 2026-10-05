using Content.Shared.Power;
using Content.Shared.SMES;
using Robust.Client.GameObjects;

namespace Content.Client.Power.SMES;

public sealed partial class SmesVisualizerSystem : VisualizerSystem<SmesComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, SmesComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, SmesVisuals.LastChargeLevel, out var level, args.Component) || level == 0)
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), SmesVisualLayers.Charge, false);
        }
        else
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), SmesVisualLayers.Charge, true);
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Charge, $"{comp.ChargeOverlayPrefix}{level}");
        }

        if (!AppearanceSystem.TryGetData<ChargeState>(uid, SmesVisuals.LastChargeState, out var state, args.Component))
            state = ChargeState.Still;

        switch (state)
        {
            case ChargeState.Still:
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Charging:
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}1");
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}1");
                break;
            case ChargeState.Discharging:
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Input, $"{comp.InputOverlayPrefix}0");
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), SmesVisualLayers.Output, $"{comp.OutputOverlayPrefix}2");
                break;
        }
    }
}

enum SmesVisualLayers : byte
{
    Input,
    Charge,
    Output,
}
