using System.Numerics;
using Content.Client.Administration.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.Administration.Systems;

public sealed partial class KillSignSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<KillSignComponent, ComponentStartup>(KillSignAdded);
        SubscribeLocalEvent<KillSignComponent, ComponentShutdown>(KillSignRemoved);
    }

    private void KillSignRemoved(EntityUid uid, KillSignComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet(sprite.AsEntity(), KillSignKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer(sprite.AsEntity(), layer);
    }

    private void KillSignAdded(EntityUid uid, KillSignComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_sprite.LayerMapTryGet(sprite.AsEntity(), KillSignKey.Key, out var _, false))
            return;

        var adj = _sprite.GetLocalBounds(sprite.AsEntityComp()).Height / 2 + ((1.0f / 32) * 6.0f);

        var layer = _sprite.AddLayer(sprite.AsEntity(), new SpriteSpecifier.Rsi(new ResPath("Objects/Misc/killsign.rsi"), "sign"));
        _sprite.LayerMapSet(sprite.AsEntity(), KillSignKey.Key, layer);

        _sprite.LayerSetOffset(sprite.AsEntity(), layer, new Vector2(0.0f, adj));
        sprite.LayerSetShader(layer, "unshaded");
    }

    private enum KillSignKey
    {
        Key,
    }
}
