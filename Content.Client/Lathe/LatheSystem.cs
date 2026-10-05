using Robust.Client.GameObjects;
using Content.Shared.Lathe;
using Content.Shared.Power;
using Content.Client.Power;
using Content.Shared.Research.Prototypes;

namespace Content.Client.Lathe;

public sealed partial class LatheSystem : SharedLatheSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LatheComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, LatheComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Lathe specific stuff
        if (_appearance.TryGetData<bool>(uid, LatheVisuals.IsRunning, out var isRunning, args.Component))
        {
            if (_sprite.LayerMapTryGet(args.Sprite.AsEntity(), LatheVisualLayers.IsRunning, out var runningLayer, false) &&
                component.RunningState != null &&
                component.IdleState != null)
            {
                var state = isRunning ? component.RunningState : component.IdleState;
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), runningLayer, state);
            }
        }

        if (_appearance.TryGetData<bool>(uid, PowerDeviceVisuals.Powered, out var powered, args.Component) &&
            _sprite.LayerMapTryGet(args.Sprite.AsEntity(), PowerDeviceVisualLayers.Powered, out var powerLayer, false))
        {
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), powerLayer, powered);

            if (component.UnlitIdleState != null &&
                component.UnlitRunningState != null)
            {
                var state = isRunning ? component.UnlitRunningState : component.UnlitIdleState;
                _sprite.LayerSetRsiState(args.Sprite.AsEntity(), powerLayer, state);
            }
        }
    }

    ///<remarks>
    /// Whether or not a recipe is available is not really visible to the client,
    /// so it just defaults to true.
    ///</remarks>
    protected override bool HasRecipe(EntityUid uid, LatheRecipePrototype recipe, LatheComponent component)
    {
        return true;
    }
}

public enum LatheVisualLayers : byte
{
    IsRunning
}
