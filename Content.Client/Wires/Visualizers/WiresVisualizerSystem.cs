using Content.Shared.Wires;
using Robust.Client.GameObjects;

namespace Content.Client.Wires.Visualizers
{
    public sealed partial class WiresVisualizerSystem : VisualizerSystem<WiresVisualsComponent>
    {
        [Dependency] private SpriteSystem _sprite = default!;
        protected override void OnAppearanceChange(EntityUid uid, WiresVisualsComponent component, ref AppearanceChangeEvent args)
        {
            if (args.Sprite == null)
                return;

            var layer = _sprite.LayerMapReserve(args.Sprite.AsEntity(), WiresVisualLayers.MaintenancePanel);

            if (args.AppearanceData.TryGetValue(WiresVisuals.MaintenancePanelState, out var panelStateObject) &&
                panelStateObject is bool panelState)
            {
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, panelState);
            }
            else
            {
                //Mainly for spawn window
                _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, false);
            }
        }
    }

    public enum WiresVisualLayers : byte
    {
        MaintenancePanel
    }
}
