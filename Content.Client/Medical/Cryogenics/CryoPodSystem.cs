using System.Numerics;
using Content.Shared.Emag.Systems;
using Content.Shared.Medical.Cryogenics;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.Medical.Cryogenics;

public sealed partial class CryoPodSystem : SharedCryoPodSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryoPodComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CryoPodComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<CryoPodComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<CryoPodComponent, GotUnEmaggedEvent>(OnUnemagged); // Frontier
        SubscribeLocalEvent<CryoPodComponent, CryoPodPryFinished>(OnCryoPodPryFinished);

        SubscribeLocalEvent<CryoPodComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<InsideCryoPodComponent, ComponentStartup>(OnCryoPodInsertion);
        SubscribeLocalEvent<InsideCryoPodComponent, ComponentRemove>(OnCryoPodRemoval);
    }

    private void OnCryoPodInsertion(EntityUid uid, InsideCryoPodComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var spriteComponent))
        {
            return;
        }

        component.PreviousOffset = spriteComponent.Offset;
        _sprite.SetOffset(spriteComponent.AsEntity(), new Vector2(0, 1));
    }

    private void OnCryoPodRemoval(EntityUid uid, InsideCryoPodComponent component, ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(uid, out var spriteComponent))
        {
            return;
        }

        _sprite.SetOffset(spriteComponent.AsEntity(), component.PreviousOffset);
    }

    private void OnAppearanceChange(EntityUid uid, CryoPodComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
        {
            return;
        }

        if (!_appearance.TryGetData<bool>(uid, CryoPodComponent.CryoPodVisuals.ContainsEntity, out var isOpen, args.Component)
            || !_appearance.TryGetData<bool>(uid, CryoPodComponent.CryoPodVisuals.IsOn, out var isOn, args.Component))
        {
            return;
        }

        if (isOpen)
        {
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), CryoPodVisualLayers.Base, "pod-open");
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), CryoPodVisualLayers.Cover, false);
            _sprite.SetDrawDepth(args.Sprite.AsEntity(), (int)DrawDepth.Objects);
        }
        else
        {
            _sprite.SetDrawDepth(args.Sprite.AsEntity(), (int)DrawDepth.Mobs);
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), CryoPodVisualLayers.Base, isOn ? "pod-on" : "pod-off");
            _sprite.LayerSetRsiState(args.Sprite.AsEntity(), CryoPodVisualLayers.Cover, isOn ? "cover-on" : "cover-off");
            _sprite.LayerSetVisible(args.Sprite.AsEntity(), CryoPodVisualLayers.Cover, true);
        }
    }
}

public enum CryoPodVisualLayers : byte
{
    Base,
    Cover,
}
