using Content.Shared.Gravity;
using Content.Shared.Power;
using Robust.Client.GameObjects;

namespace Content.Client.Gravity;

public sealed partial class GravitySystem : SharedGravitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private AppearanceSystem _appearanceSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SharedGravityGeneratorComponent, AppearanceChangeEvent>(OnAppearanceChange);
        InitializeShake();
    }

    /// <summary>
    /// Ensures that the visible state of gravity generators are synced with their sprites.
    /// </summary>
    private void OnAppearanceChange(EntityUid uid, SharedGravityGeneratorComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearanceSystem.TryGetData<PowerChargeStatus>(uid, PowerChargeVisuals.State, out var state, args.Component))
        {
            if (comp.SpriteMap.TryGetValue(state, out var spriteState))
            {
                var layer = _sprite.LayerMapGet(args.Sprite.AsEntity(), GravityGeneratorVisualLayers.Base);
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, spriteState);
            }
        }

        if (_appearanceSystem.TryGetData<float>(uid, PowerChargeVisuals.Charge, out var charge, args.Component))
        {
            var layer = _sprite.LayerMapGet(args.Sprite.AsEntity(), GravityGeneratorVisualLayers.Core);
            switch (charge)
            {
                case < 0.2f:
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, false);
                    break;
                case >= 0.2f and < 0.4f:
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, comp.CoreStartupState);
                    break;
                case >= 0.4f and < 0.6f:
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, comp.CoreIdleState);
                    break;
                case >= 0.6f and < 0.8f:
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, comp.CoreActivatingState);
                    break;
                default:
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, comp.CoreActivatedState);
                    break;
            }
        }
    }
}

public enum GravityGeneratorVisualLayers : byte
{
    Base,
    Core
}
