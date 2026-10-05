using Content.Shared.Storage;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Visualizers;

public sealed partial class EntityStorageVisualizerSystem : VisualizerSystem<EntityStorageVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntityStorageVisualsComponent, ComponentInit>(OnComponentInit);
    }

    /// <summary>
    /// Sets the base sprite to this layer. Exists to make the inheritance tree less boilerplate-y.
    /// </summary>
    private void OnComponentInit(EntityUid uid, EntityStorageVisualsComponent comp, ComponentInit args)
    {
        if (comp.StateBaseClosed == null)
            return;

        comp.StateBaseOpen ??= comp.StateBaseClosed;
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetRsiState(sprite.AsEntity(), StorageVisualLayers.Base, comp.StateBaseClosed);
    }

    protected override void OnAppearanceChange(EntityUid uid, EntityStorageVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
        || !AppearanceSystem.TryGetData<bool>(uid, StorageVisuals.Open, out var open, args.Component))
            return;

        // Open/Closed state for the storage entity.
        if (_sprite.LayerMapTryGet(args.Sprite.AsEntity(), StorageVisualLayers.Door, out _, false))
        {
            if (open)
            {
                if (comp.OpenDrawDepth != null)
                    _sprite.SetDrawDepth(args.Sprite.AsEntity(), comp.OpenDrawDepth.Value);

                if (comp.StateDoorOpen != null)
                {
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), StorageVisualLayers.Door, comp.StateDoorOpen);
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), StorageVisualLayers.Door, true);
                }
                else
                {
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), StorageVisualLayers.Door, false);
                }

                if (comp.StateBaseOpen != null)
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), StorageVisualLayers.Base, comp.StateBaseOpen);
            }
            else
            {
                if (comp.ClosedDrawDepth != null)
                    _sprite.SetDrawDepth(args.Sprite.AsEntity(), comp.ClosedDrawDepth.Value);

                if (comp.StateDoorClosed != null)
                {
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), StorageVisualLayers.Door, comp.StateDoorClosed);
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), StorageVisualLayers.Door, true);
                }
                else
                    _sprite.LayerSetVisible(args.Sprite.AsEntity(), StorageVisualLayers.Door, false);

                if (comp.StateBaseClosed != null)
                    _sprite.LayerSetRsiState(args.Sprite.AsEntity(), StorageVisualLayers.Base, comp.StateBaseClosed);
            }
        }
    }
}

public enum StorageVisualLayers : byte
{
    Base,
    Door
}
