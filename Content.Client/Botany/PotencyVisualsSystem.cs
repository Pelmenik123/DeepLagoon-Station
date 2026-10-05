using System.Numerics;
using Content.Shared.Botany;
using Content.Client.Botany.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Botany;

public sealed partial class PotencyVisualsSystem : VisualizerSystem<PotencyVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, PotencyVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<float>(uid, ProduceVisuals.Potency, out var potency, args.Component))
        {
            var scale = MathHelper.Lerp(component.MinimumScale, component.MaximumScale, potency / 100);
            _sprite.SetScale(args.Sprite.AsEntity(), new Vector2(scale, scale));
        }
    }
}
