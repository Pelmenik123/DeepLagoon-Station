// SPDX-FileCopyrightText: 2022 Alex Evgrashin
// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2023 Visne
// SPDX-FileCopyrightText: 2025 metalgearsloth
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Visualizers;

public sealed partial class StorageFillVisualizerSystem : VisualizerSystem<StorageFillVisualizerComponent>
{
    [Dependency] private SpriteSystem _sprite = default!;
    protected override void OnAppearanceChange(EntityUid uid, StorageFillVisualizerComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageFillVisuals.FillLevel, out var level, args.Component))
            return;

        var state = $"{component.FillBaseName}-{level}";
        _sprite.LayerSetRsiState(args.Sprite.AsEntity(), StorageFillLayers.Fill, state);
    }
}
