using System.Numerics;
using Content.Shared.IconSmoothing;
using Robust.Client.GameObjects;

namespace Content.Client.IconSmoothing;

public sealed partial class IconSmoothSystem
{
    // Handles drawing edge sprites on the non-smoothed edges.

    private void InitializeEdge()
    {
        SubscribeLocalEvent<SmoothEdgeComponent, ComponentStartup>(OnEdgeStartup);
        SubscribeLocalEvent<SmoothEdgeComponent, ComponentShutdown>(OnEdgeShutdown);
    }

    private void OnEdgeStartup(EntityUid uid, SmoothEdgeComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetOffset(sprite.AsEntity(), EdgeLayer.South, new Vector2(0, -1f));
        _sprite.LayerSetOffset(sprite.AsEntity(), EdgeLayer.East, new Vector2(1f, 0f));
        _sprite.LayerSetOffset(sprite.AsEntity(), EdgeLayer.North, new Vector2(0, 1f));
        _sprite.LayerSetOffset(sprite.AsEntity(), EdgeLayer.West, new Vector2(-1f, 0f));

        _sprite.LayerSetVisible(sprite.AsEntity(), EdgeLayer.South, false);
        _sprite.LayerSetVisible(sprite.AsEntity(), EdgeLayer.East, false);
        _sprite.LayerSetVisible(sprite.AsEntity(), EdgeLayer.North, false);
        _sprite.LayerSetVisible(sprite.AsEntity(), EdgeLayer.West, false);
    }

    private void OnEdgeShutdown(EntityUid uid, SmoothEdgeComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerMapRemove(sprite.AsEntity(), EdgeLayer.South);
        _sprite.LayerMapRemove(sprite.AsEntity(), EdgeLayer.East);
        _sprite.LayerMapRemove(sprite.AsEntity(), EdgeLayer.North);
        _sprite.LayerMapRemove(sprite.AsEntity(), EdgeLayer.West);
    }

    private void CalculateEdge(EntityUid uid, DirectionFlag directions, SpriteComponent? sprite = null, SmoothEdgeComponent? component = null)
    {
        if (!Resolve(uid, ref sprite, ref component, false))
            return;

        for (var i = 0; i < 4; i++)
        {
            var dir = (DirectionFlag)Math.Pow(2, i);
            var edge = GetEdge(dir);

            if ((dir & directions) != 0x0)
            {
                _sprite.LayerSetVisible(sprite.AsEntity(), edge, false);
                continue;
            }

            _sprite.LayerSetVisible(sprite.AsEntity(), edge, true);
        }
    }

    private EdgeLayer GetEdge(DirectionFlag direction)
    {
        switch (direction)
        {
            case DirectionFlag.South:
                return EdgeLayer.South;
            case DirectionFlag.East:
                return EdgeLayer.East;
            case DirectionFlag.North:
                return EdgeLayer.North;
            case DirectionFlag.West:
                return EdgeLayer.West;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private enum EdgeLayer : byte
    {
        South,
        East,
        North,
        West
    }
}
