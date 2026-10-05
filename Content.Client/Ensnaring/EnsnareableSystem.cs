using Content.Shared.Ensnaring;
using Robust.Shared.Utility;
using Content.Shared.Ensnaring.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Ensnaring;

public sealed partial class EnsnareableSystem : SharedEnsnareableSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnsnareableComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    protected override void OnEnsnareInit(Entity<EnsnareableComponent> ent, ref ComponentInit args)
    {
        base.OnEnsnareInit(ent, ref args);

        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite))
            return;

        // TODO remove this, this should just be in yaml.
        _sprite.LayerMapReserve(sprite.AsEntity(), EnsnaredVisualLayers.Ensnared);
    }

    private void OnAppearanceChange(EntityUid uid, EnsnareableComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !_sprite.LayerMapTryGet(args.Sprite.AsEntity(), EnsnaredVisualLayers.Ensnared, out var layer, false))
            return;

        if (_appearance.TryGetData<bool>(uid, EnsnareableVisuals.IsEnsnared, out var isEnsnared, args.Component))
        {
            if (component.Sprite != null)
            {
                _sprite.LayerSetRsi(args.Sprite.AsEntity(), layer, new ResPath(component.Sprite));
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, component.State);
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, isEnsnared);
            }
        }
    }
}

public enum EnsnaredVisualLayers : byte
{
    Ensnared,
}
