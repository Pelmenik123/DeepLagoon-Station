// SPDX-FileCopyrightText: 2021 ShadowCommander
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto
// SPDX-FileCopyrightText: 2021 Visne
// SPDX-FileCopyrightText: 2022 Acruid
// SPDX-FileCopyrightText: 2022 Flipp Syder
// SPDX-FileCopyrightText: 2022 Nemanja
// SPDX-FileCopyrightText: 2022 metalgearsloth
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 themias
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 Ben
// SPDX-FileCopyrightText: 2023 BenOwnby
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 Leon Friedrich
// SPDX-FileCopyrightText: 2023 keronshb
// SPDX-FileCopyrightText: 2024 Plykiya
// SPDX-FileCopyrightText: 2024 Tayrtahn
// SPDX-FileCopyrightText: 2024 Whatstone
// SPDX-FileCopyrightText: 2024 Winkarst
// SPDX-FileCopyrightText: 2024 nikthechampiongr
// SPDX-FileCopyrightText: 2025 J
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Engineering.Components;
using Content.Server.Stack;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.DoAfter;
using Content.Shared.Engineering;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Stacks;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Engineering.EntitySystems
{
    [UsedImplicitly]
    public sealed partial class SpawnAfterInteractSystem : EntitySystem
    {
        [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private StackSystem _stackSystem = default!;
        [Dependency] private TurfSystem _turfSystem = default!;
        [Dependency] private SharedTransformSystem _transform = default!;
        [Dependency] private SharedMapSystem _map = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpawnAfterInteractComponent, AfterInteractEvent>(HandleAfterInteract);
            SubscribeLocalEvent<SpawnAfterInteractComponent, SpawnAfterInteractDoAfterEvent>(OnSpawnAfterInteractDoAfter);
        }

        private void HandleAfterInteract(EntityUid uid, SpawnAfterInteractComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach && !component.IgnoreDistance)
                return;
            if (string.IsNullOrEmpty(component.Prototype))
                return;
            if (!TryComp<MapGridComponent>(_transform.GetGrid(args.ClickLocation), out var grid))
                return;
            var gridUid = _transform.GetGrid(args.ClickLocation)!.Value;
            if (!_map.TryGetTileRef(gridUid, grid, args.ClickLocation, out var tileRef))
                return;

            bool IsTileClear()
            {
                return tileRef.Tile.IsEmpty == false && !_turfSystem.IsTileBlocked(tileRef, CollisionGroup.MobMask);
            }

            if (!IsTileClear())
                return;

            if (component.DoAfterTime > 0)
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.DoAfterTime, new SpawnAfterInteractDoAfterEvent(EntityManager, args.ClickLocation), uid)
                {
                    BreakOnMove = true,
                };

                _doAfterSystem.TryStartDoAfter(doAfterArgs);
                return;
            }

            FinishSpawn(uid, component, args.User, args.ClickLocation);
        }

        private void OnSpawnAfterInteractDoAfter(EntityUid uid, SpawnAfterInteractComponent component, SpawnAfterInteractDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled)
                return;

            FinishSpawn(uid, component, args.User, GetCoordinates(args.ClickLocation));
        }

        private void FinishSpawn(EntityUid uid, SpawnAfterInteractComponent component, EntityUid user, EntityCoordinates clickLocation)
        {
            if (component.Deleted || Deleted(uid) || Deleted(user))
                return;

            if (!TryComp<MapGridComponent>(_transform.GetGrid(clickLocation), out var grid))
                return;

            var gridUid = _transform.GetGrid(clickLocation)!.Value;
            if (!_map.TryGetTileRef(gridUid, grid, clickLocation, out var tileRef))
                return;

            if (tileRef.Tile.IsEmpty || _turfSystem.IsTileBlocked(tileRef, CollisionGroup.MobMask))
                return;

            if (TryComp(uid, out StackComponent? stackComp)
                && component.RemoveOnInteract && !_stackSystem.Use(uid, 1, stackComp))
            {
                return;
            }

            Spawn(component.Prototype, clickLocation.SnapToGrid(grid));

            if (component.RemoveOnInteract && stackComp == null)
                QueueDel(uid); // Frontier: TryQueueDel<QueueDel
        }
    }
}
