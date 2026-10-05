using Content.Shared.Singularity.Components;
using Content.Shared.Singularity.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Client.Singularity.Systems;

public sealed partial class EmitterSystem : SharedEmitterSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<EmitterComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, EmitterComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<EmitterVisualState>(uid, EmitterVisuals.VisualState, out var state, args.Component))
            state = EmitterVisualState.Off;

        if (!_sprite.LayerMapTryGet(args.Sprite.AsEntity(), EmitterVisualLayers.Lights, out var layer, false))
            return;

        switch (state)
        {
            case EmitterVisualState.On:
                if (component.OnState == null)
                    break;
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, component.OnState);
                break;
            case EmitterVisualState.Underpowered:
                if (component.UnderpoweredState == null)
                    break;
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, true);
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, component.UnderpoweredState);
                break;
            case EmitterVisualState.Off:
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
