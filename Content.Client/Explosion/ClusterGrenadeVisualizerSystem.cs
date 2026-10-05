using Content.Shared.Explosion;
using Content.Shared.Explosion.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Explosion;

public sealed partial class ClusterGrenadeVisualizerSystem : VisualizerSystem<ClusterGrenadeVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, ClusterGrenadeVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<int>(uid, ClusterGrenadeVisuals.GrenadesCounter, out var grenadesCounter, args.Component))
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), 0, $"{comp.State}-{grenadesCounter}");
    }
}
