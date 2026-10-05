using System.Linq;
using Content.Shared.AlertLevel;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client.AlertLevel;

public sealed partial class AlertLevelDisplaySystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlertLevelDisplayComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, AlertLevelDisplayComponent alertLevelDisplay, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
        {
            return;
        }
        var layer = _sprite.LayerMapReserve(args.Sprite.AsEntity(), AlertLevelDisplay.Layer);

        if (args.AppearanceData.TryGetValue(AlertLevelDisplay.Powered, out var poweredObject))
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, poweredObject is true);
        }

        if (!args.AppearanceData.TryGetValue(AlertLevelDisplay.CurrentLevel, out var level))
        {
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, alertLevelDisplay.AlertVisuals.Values.First());
            return;
        }

        if (alertLevelDisplay.AlertVisuals.TryGetValue((string)level, out var visual))
        {
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, visual);
        }
        else
        {
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, alertLevelDisplay.AlertVisuals.Values.First());
        }
    }
}
