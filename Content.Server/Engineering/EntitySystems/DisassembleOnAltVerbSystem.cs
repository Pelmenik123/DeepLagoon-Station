using Content.Server.Engineering.Components;
using Content.Shared.DoAfter;
using Content.Shared.Engineering;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using JetBrains.Annotations;

namespace Content.Server.Engineering.EntitySystems
{
    [UsedImplicitly]
    public sealed partial class DisassembleOnAltVerbSystem : EntitySystem
    {
        [Dependency] private SharedHandsSystem _handsSystem = default!;
        [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DisassembleOnAltVerbComponent, GetVerbsEvent<AlternativeVerb>>(AddDisassembleVerb);
            SubscribeLocalEvent<DisassembleOnAltVerbComponent, DisassembleOnAltVerbDoAfterEvent>(OnDisassembleDoAfter);
        }
        private void AddDisassembleVerb(EntityUid uid, DisassembleOnAltVerbComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || args.Hands == null)
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    AttemptDisassemble(uid, args.User, args.Target, component);
                },
                Text = Loc.GetString("disassemble-system-verb-disassemble"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        public void AttemptDisassemble(EntityUid uid, EntityUid user, EntityUid target, DisassembleOnAltVerbComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;
            if (string.IsNullOrEmpty(component.Prototype))
                return;

            if (component.DoAfterTime > 0)
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, user, component.DoAfterTime, new DisassembleOnAltVerbDoAfterEvent(), uid)
                {
                    BreakOnMove = true,
                };

                _doAfterSystem.TryStartDoAfter(doAfterArgs);
                return;
            }

            FinishDisassemble(uid, user, component);
        }

        private void OnDisassembleDoAfter(EntityUid uid, DisassembleOnAltVerbComponent component, DisassembleOnAltVerbDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled)
                return;

            FinishDisassemble(uid, args.User, component);
        }

        private void FinishDisassemble(EntityUid uid, EntityUid user, DisassembleOnAltVerbComponent component)
        {
            if (component.Deleted || Deleted(uid) || Deleted(user))
                return;

            if (!TryComp(uid, out TransformComponent? transformComp))
                return;

            var entity = Spawn(component.Prototype, transformComp.Coordinates);

            _handsSystem.TryPickup(user, entity);

            Del(uid);
        }
    }
}
