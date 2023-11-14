﻿// -----------------------------------------------------------------------
// <copyright file="UsingItemPrefix.cs" company="Lethal Company Modding Community">
// Copyright (c) Lethal Company Modding Community. All rights reserved.
// Licensed under the GPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace LCAPI.Core.Events.Patches.Events;

using Attributes;
using EventArgs.Player;

/// <summary>
///     Patches the <see cref="Handlers.Player.UsingItem"/> event.
/// </summary>
[EventPatch(typeof(Handlers.Player), nameof(Handlers.Player.UsingKey))]
[HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.ItemActivate))]
internal static class UsingItemPrefix
{
    [HarmonyPrefix]
    private static bool Prefix(GrabbableObject __instance, bool used, bool buttonDown = true)
    {
        // This needs to become a transpiler.
        UsingItemEventArgs ev = new UsingItemEventArgs(__instance);
        Handlers.Player.OnUsingItem(ev);
        return ev.IsAllowed;
    }
}