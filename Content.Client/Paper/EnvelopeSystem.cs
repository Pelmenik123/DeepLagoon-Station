using Content.Shared.Paper;
using Robust.Client.GameObjects;

namespace Content.Client.Paper;

public sealed partial class EnvelopeSystem : VisualizerSystem<EnvelopeComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EnvelopeComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<EnvelopeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(ent);
    }

    private void UpdateAppearance(Entity<EnvelopeComponent> ent, SpriteComponent? sprite = null)
    {
        if (!Resolve(ent.Owner, ref sprite))
            return;

        _sprite.LayerSetVisible(sprite.AsEntity(), EnvelopeVisualLayers.Open, ent.Comp.State == EnvelopeComponent.EnvelopeState.Open);
        _sprite.LayerSetVisible(sprite.AsEntity(), EnvelopeVisualLayers.Sealed, ent.Comp.State == EnvelopeComponent.EnvelopeState.Sealed);
        _sprite.LayerSetVisible(sprite.AsEntity(), EnvelopeVisualLayers.Torn, ent.Comp.State == EnvelopeComponent.EnvelopeState.Torn);
    }

    public enum EnvelopeVisualLayers : byte
    {
        Open,
        Sealed,
        Torn
    }
}
