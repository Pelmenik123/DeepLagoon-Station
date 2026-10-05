using Content.Shared.SurveillanceCamera;
using Robust.Client.GameObjects;

namespace Content.Client.SurveillanceCamera;

public sealed partial class SurveillanceCameraVisualsSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurveillanceCameraVisualsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, SurveillanceCameraVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(SurveillanceCameraVisualsKey.Key, out var data)
            || data is not SurveillanceCameraVisuals key
            || args.Sprite == null
            || !_sprite.LayerMapTryGet(args.Sprite.AsEntity(), SurveillanceCameraVisualsKey.Layer, out int layer, false)
            || !component.CameraSprites.TryGetValue(key, out var state))
        {
            return;
        }

        _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, state);
    }
}
