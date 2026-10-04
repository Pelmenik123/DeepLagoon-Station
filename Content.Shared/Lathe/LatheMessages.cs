// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2019 Víctor Aguilera Puerto
// SPDX-FileCopyrightText: 2019 ZelteHonor
// SPDX-FileCopyrightText: 2020 FL-OZ
// SPDX-FileCopyrightText: 2021 Acruid
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto
// SPDX-FileCopyrightText: 2021 Visne
// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2022 Nemanja
// SPDX-FileCopyrightText: 2022 Rane
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2025 Ilya246
// SPDX-FileCopyrightText: 2025 Whatstone
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Lathe;

[Serializable, NetSerializable]
public sealed class LatheUpdateState(List<ProtoId<LatheRecipePrototype>> recipes, List<LatheRecipeBatch> queue, ProtoId<LatheRecipePrototype>? currentlyProducing = null, bool looping = false, bool skipping = false) : BoundUserInterfaceState
{
    public List<ProtoId<LatheRecipePrototype>> Recipes = recipes;

    public List<LatheRecipeBatch> Queue = queue; // Frontier: LatheRecipePrototype<LatheRecipeBatch

    public ProtoId<LatheRecipePrototype>? CurrentlyProducing = currentlyProducing;

    public bool Looping = looping; // Mono
    public bool Skipping = skipping; // Mono
}

/// <summary>
///     Sent to the server to sync material storage and the recipe queue.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSyncRequestMessage : BoundUserInterfaceMessage
{

}

/// <summary>
///     Sent to the server when a client queues a new recipe.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheQueueRecipeMessage(string id, int quantity) : BoundUserInterfaceMessage
{
    public readonly string ID = id;
    public readonly int Quantity = quantity;
}

// Mono
/// <summary>
///     Sent to the server when a client wants to change whether the lathe should loop.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSetLoopingMessage(bool shouldLoop) : BoundUserInterfaceMessage
{
    public readonly bool ShouldLoop = shouldLoop;
}

// Mono
/// <summary>
///     Sent to the server when a client wants to change whether the lathe should skip over unavailable recipes.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheSetSkipMessage(bool shouldSkip) : BoundUserInterfaceMessage
{
    public readonly bool ShouldSkip = shouldSkip;
}

// Mono
/// <summary>
///     Sent to the server when a client wants to de-queue a recipe from the lathe.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheRecipeCancelMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

[NetSerializable, Serializable]
public enum LatheUiKey
{
    Key,
}
