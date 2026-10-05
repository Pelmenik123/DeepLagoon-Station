using Content.Client.Storage.Components;
using Content.Shared.Rounding;
using Content.Shared.Storage;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Systems;

/// <inheritdoc cref="StorageContainerVisualsComponent"/>
public sealed partial class StorageContainerVisualsSystem : VisualizerSystem<StorageContainerVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, StorageContainerVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageVisuals.StorageUsed, out var used, args.Component))
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageVisuals.Capacity, out var capacity, args.Component))
            return;

        var fraction = used / (float)capacity;

        if (!_sprite.LayerMapTryGet(args.Sprite.AsEntity(), component.FillLayer, out var fillLayer, false))
            return;

        var closestFillSprite = Math.Min(ContentHelpers.RoundToNearestLevels(fraction, 1, component.MaxFillLevels + 1),
            component.MaxFillLevels);

        if (closestFillSprite > 0)
        {
            if (component.FillBaseName == null)
                return;

            _sprite.LayerSetVisible(args.Sprite.AsEntity(), fillLayer, true);
            var stateName = component.FillBaseName + closestFillSprite;
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), fillLayer, stateName);
        }
        else
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), fillLayer, false);
        }
    }
}
