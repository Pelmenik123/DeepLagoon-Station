using Content.Client.Botany.Components;
using Robust.Shared.Utility;
using Content.Shared.Botany;
using Robust.Client.GameObjects;

namespace Content.Client.Botany;

public sealed partial class PlantHolderVisualizerSystem : VisualizerSystem<PlantHolderVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlantHolderVisualsComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, PlantHolderVisualsComponent component, ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerMapReserve(sprite.AsEntity(), PlantHolderLayers.Plant);
        _sprite.LayerSetVisible(sprite.AsEntity(), PlantHolderLayers.Plant, false);
    }

    protected override void OnAppearanceChange(EntityUid uid, PlantHolderVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, PlantHolderVisuals.PlantRsi, out var rsi, args.Component)
            && AppearanceSystem.TryGetData<string>(uid, PlantHolderVisuals.PlantState, out var state, args.Component))
        {
            var valid = !string.IsNullOrWhiteSpace(state);

            _sprite.LayerSetVisible(args.Sprite.AsEntity(), PlantHolderLayers.Plant, valid);

            if (valid)
            {
                _sprite.LayerSetRsi(args.Sprite.AsEntity(), PlantHolderLayers.Plant, new ResPath(rsi));
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), PlantHolderLayers.Plant, state);
            }
        }
    }
}

public enum PlantHolderLayers : byte
{
    Plant,
    HealthLight,
    WaterLight,
    NutritionLight,
    AlertLight,
    HarvestLight,
}
