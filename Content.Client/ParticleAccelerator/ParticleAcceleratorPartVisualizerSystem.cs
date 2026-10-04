using System.Linq;
using Content.Shared.Singularity.Components;
using Robust.Client.GameObjects;

namespace Content.Client.ParticleAccelerator;

public sealed partial class ParticleAcceleratorPartVisualizerSystem : VisualizerSystem<ParticleAcceleratorPartVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, ParticleAcceleratorPartVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_sprite.LayerMapTryGet(args.Sprite.AsEntity(), ParticleAcceleratorVisualLayers.Unlit, out var index, false))
            return;

        if (!AppearanceSystem.TryGetData<ParticleAcceleratorVisualState>(uid, ParticleAcceleratorVisuals.VisualState, out var state, args.Component))
        {
            state = ParticleAcceleratorVisualState.Unpowered;
        }

        if (state != ParticleAcceleratorVisualState.Unpowered)
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), index, true);
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), index, comp.StateBase + comp.StatesSuffixes[state]);
        }
        else
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), index, false);
        }
    }
}
