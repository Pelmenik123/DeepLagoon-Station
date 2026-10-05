using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Client.Nutrition.EntitySystems;

public sealed partial class ClientFoodSequenceSystem : SharedFoodSequenceSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<FoodSequenceStartPointComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<FoodSequenceStartPointComponent> start, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(start, out var sprite))
            return;

        UpdateFoodVisuals(start, sprite);
    }

    private void UpdateFoodVisuals(Entity<FoodSequenceStartPointComponent> start, SpriteComponent? sprite = null)
    {
        if (!Resolve(start, ref sprite, false))
            return;

        //Remove old layers
        foreach (var key in start.Comp.RevealedLayers)
        {
            _sprite.RemoveLayer(sprite.AsEntity(), key);
        }
        start.Comp.RevealedLayers.Clear();

        //Add new layers
        var counter = 0;
        foreach (var state in start.Comp.FoodLayers)
        {
            if (state.Sprite is null)
                continue;

            var keyCode = $"food-layer-{counter}";
            start.Comp.RevealedLayers.Add(keyCode);

            _sprite.LayerMapTryGet(sprite.AsEntity(), start.Comp.TargetLayerMap, out var index, false);

            if (start.Comp.InverseLayers)
                index++;

            _sprite.AddBlankLayer(sprite.AsEntityComp(), index);
            _sprite.LayerMapSet(sprite.AsEntity(), keyCode, index);
            _sprite.LayerSetSprite(sprite.AsEntity(), index, state.Sprite);
            _sprite.LayerSetScale(sprite.AsEntity(), index, state.Scale);

            //Offset the layer
            var layerPos = start.Comp.StartPosition;
            layerPos += (start.Comp.Offset * counter) + state.LocalOffset;
            _sprite.LayerSetOffset(sprite.AsEntity(), index, layerPos);

            counter++;
        }
    }
}
