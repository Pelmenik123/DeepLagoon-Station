using Content.Shared.Storage;
using Content.Shared.Lock;
using Robust.Client.GameObjects;

namespace Content.Client.Lock.Visualizers;

public sealed partial class LockVisualizerSystem : VisualizerSystem<LockVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, LockVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !AppearanceSystem.TryGetData<bool>(uid, LockVisuals.Locked, out _, args.Component))
            return;

        // Lock state for the entity.
        if (!AppearanceSystem.TryGetData<bool>(uid, LockVisuals.Locked, out var locked, args.Component))
            locked = true;

        var unlockedStateExist = args.Sprite.BaseRSI?.TryGetState(comp.StateUnlocked, out _);

        if (AppearanceSystem.TryGetData<bool>(uid, StorageVisuals.Open, out var open, args.Component))
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), LockVisualLayers.Lock, !open);
        }
        else if (!(bool)unlockedStateExist!)
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), LockVisualLayers.Lock, locked);

        if (!open && (bool)unlockedStateExist!)
        {
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), LockVisualLayers.Lock, locked ? comp.StateLocked : comp.StateUnlocked);
        }
    }
}

public enum LockVisualLayers : byte
{
    Lock
}
