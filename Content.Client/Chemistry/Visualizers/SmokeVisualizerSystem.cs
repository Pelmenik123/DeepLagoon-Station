using Content.Shared.Smoking;
using Robust.Client.GameObjects;

namespace Content.Client.Chemistry.Visualizers;

/// <summary>
/// Ensures entities with <see cref="SmokeVisualsComponent"/> have a color corresponding with their contained reagents.
/// </summary>
public sealed partial class SmokeVisualizerSystem : VisualizerSystem<SmokeVisualsComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    /// <summary>
    /// Syncs the color of the smoke with the color of its contained reagents.
    /// </summary>
    protected override void OnAppearanceChange(EntityUid uid, SmokeVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (!AppearanceSystem.TryGetData<Color>(uid, SmokeVisuals.Color, out var color))
            return;
        _sprite.SetColor(args.Sprite.AsEntity(), color);
    }
}
