using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.IgnoreHumanoids;

/// <summary>
/// Stops drones from telling people apart.
/// </summary>
public sealed class IgnoreHumanoidsOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly SharedTransformSystem _transform;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    private Dictionary<EntityUid, EntityUid> _effectList = new();

    public IgnoreHumanoidsOverlay(IEntityManager entManager, IPrototypeManager protoManager)
    {
        _entManager = entManager;
        _transform = _entManager.EntitySysManager.GetEntitySystem<SharedTransformSystem>();
    }

    /// <summary>
    /// Yeah we technically aren't directly drawing anything here.
    /// If I made it an entity system there would be some overhead, though...
    /// </summary>
    protected override void Draw(in OverlayDrawArgs args)
    {
        var spriteQuery = _entManager.GetEntityQuery<SpriteComponent>();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        var humanoidQuery = _entManager.AllEntityQueryEnumerator<HumanoidAppearanceComponent>();
        while (humanoidQuery.MoveNext(out var humanoidUid, out _))
        {
            if (!spriteQuery.TryGetComponent(humanoidUid, out var sprite))
            {
                continue;
            }

            if (!xformQuery.TryGetComponent(humanoidUid, out var xform))
            {
                continue;
            }

            if (sprite.Visible && !_effectList.ContainsKey(humanoidUid))
            {
                _entManager.System<SpriteSystem>().SetVisible(sprite.AsEntity(), false);
                var effect = _entManager.SpawnEntity("EffectUnknownHumanoid", xform.Coordinates);
                _effectList.Add(humanoidUid, effect);
            }
        }

        // surprisingly no collectionmodified CBT when I tested
        foreach (var (underlying, effect) in _effectList)
        {
            if (_entManager.Deleted(underlying))
            {
                _entManager.DeleteEntity(effect);
                _effectList.Remove(underlying);
                continue;
            }

            if (!xformQuery.TryGetComponent(underlying, out var underlyingxform))
                continue;

            if (!xformQuery.TryGetComponent(effect, out var effectxform))
                continue;

            _transform.SetLocalPositionRotation(effect, underlyingxform.LocalPosition, underlyingxform.LocalRotation);
        }
    }

    public void Reset()
    {
        foreach (var (underlying, effect) in _effectList)
        {
            _entManager.DeleteEntity(effect);
            _effectList.Remove(underlying);

            if (_entManager.TryGetComponent<SpriteComponent>(underlying, out var sprite))
                _entManager.System<SpriteSystem>().SetVisible(sprite.AsEntity(), true);
        }
    }
}
