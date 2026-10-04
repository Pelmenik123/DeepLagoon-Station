using Content.Shared.Weapons.Marker;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client.Weapons.Marker;

public sealed partial class DamageMarkerSystem : SharedDamageMarkerSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageMarkerComponent, ComponentStartup>(OnMarkerStartup);
        SubscribeLocalEvent<DamageMarkerComponent, ComponentShutdown>(OnMarkerShutdown);
    }

    private void OnMarkerStartup(EntityUid uid, DamageMarkerComponent component, ComponentStartup args)
    {
        if (!_timing.ApplyingState || component.Effect == null || !TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var layer = _sprite.LayerMapReserve(sprite.AsEntity(), DamageMarkerKey.Key);
        _sprite.LayerSetRsi(sprite.AsEntity(), layer, component.Effect.RsiPath, component.Effect.RsiState);
    }

    private void OnMarkerShutdown(EntityUid uid, DamageMarkerComponent component, ComponentShutdown args)
    {
        if (!_timing.ApplyingState || !TryComp<SpriteComponent>(uid, out var sprite) || !_sprite.LayerMapTryGet(sprite.AsEntity(), DamageMarkerKey.Key, out var weh, false))
            return;

        _sprite.RemoveLayer(sprite.AsEntity(), weh);
    }

    private enum DamageMarkerKey : byte
    {
        Key
    }
}
