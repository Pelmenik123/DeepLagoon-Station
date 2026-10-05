using Content.Shared.Chat.TypingIndicator;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;

namespace Content.Client.Chat.TypingIndicator;

public sealed partial class TypingIndicatorVisualizerSystem : VisualizerSystem<TypingIndicatorComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private InventorySystem _inventory = default!;


    protected override void OnAppearanceChange(EntityUid uid, TypingIndicatorComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var currentTypingIndicator = component.TypingIndicatorPrototype;

        var evt = new BeforeShowTypingIndicatorEvent();

        if (TryComp<InventoryComponent>(uid, out var inventoryComp))
            _inventory.RelayEvent((uid, inventoryComp), ref evt);

        var overrideIndicator = evt.GetMostRecentIndicator();

        if (overrideIndicator != null)
            currentTypingIndicator = overrideIndicator.Value;

        if (!_prototypeManager.TryIndex(currentTypingIndicator, out var proto))
        {
            Log.Error($"Unknown typing indicator id: {component.TypingIndicatorPrototype}");
            return;
        }

        AppearanceSystem.TryGetData<bool>(uid, TypingIndicatorVisuals.IsTyping, out var isTyping, args.Component);
        var layerExists = _sprite.LayerMapTryGet(args.Sprite.AsEntity(), TypingIndicatorLayers.Base, out var layer, false);
        if (!layerExists)
            layer = _sprite.LayerMapReserve(args.Sprite.AsEntity(), TypingIndicatorLayers.Base);

        _sprite.LayerSetRsi(args.Sprite.AsEntity(), layer, proto.SpritePath);
        _sprite.LayerSetRsiState(args.Sprite.AsEntity(), layer, proto.TypingState);
        args.Sprite.LayerSetShader(layer, proto.Shader);
        _sprite.LayerSetOffset(args.Sprite.AsEntity(), layer, proto.Offset);
        _sprite.LayerSetVisible(args.Sprite.AsEntity(), layer, isTyping);
    }
}
