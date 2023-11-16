﻿// -----------------------------------------------------------------------
// <copyright file="GameNetworkManagerJoinPatch.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the GPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace LethalAPI.Core.Patches;

using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;
using Models;
using Steamworks;
using Steamworks.Data;

/// <summary>
///     Patches <see cref="GameNetworkManager.SteamMatchmaking_OnLobbyCreated" />.
///     Adds extra lobby metadata to be used by the API for dependency checking.
/// </summary>
/// <seealso cref="GameNetworkManager.SteamMatchmaking_OnLobbyCreated" />
[HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyCreated")]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class SteamMatchmakingOnLobbyCreatedPostfix
{
    [HarmonyPostfix]
    private static void Postfix(Result result, ref Lobby lobby)
    {
        // lobby has not yet been created or something went wrong
        if (result != Result.OK)
        {
            return;
        }

        lobby.SetData(LobbyMetadata.Modded, "true");
        lobby.SetData(LobbyMetadata.Plugins, PluginManager.GetLobbyPluginsMetadata());
        lobby.SetData(LobbyMetadata.JoinableModded, lobby.GetData(LobbyMetadata.Joinable));
        lobby.SetData(LobbyMetadata.Name, "modded // " + lobby.GetData(LobbyMetadata.Name));

        if (PluginManager.GetAPIPluginInfoList().Exists(plugin => plugin.Required))
        {
            lobby.SetData(LobbyMetadata.Joinable, "false");
        }
    }
}

/// <summary>
///     Patches <see cref="GameNetworkManager.LobbyDataIsJoinable" />.
///     Checks if required plugins are present in the lobby metadata and are the same version as the client.
/// </summary>
/// <seealso cref="GameNetworkManager.LobbyDataIsJoinable" />
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class LobbyDataIsJoinablePostfix
{
    [HarmonyPostfix]
    private static bool Postfix(bool __result, ref Lobby lobby, GameNetworkManager __instance)
    {
        // If original result was false, return false
        if (!__result)
        {
            return false;
        }

        // If the lobby is not modded, return original result
        if (lobby.GetData(LobbyMetadata.Modded) != "true")
        {
            return __result;
        }

        string lobbyPluginString = lobby.GetData(LobbyMetadata.Plugins);

        // If the lobby does not have any plugin information, return original result
        if (string.IsNullOrEmpty(lobbyPluginString))
        {
            Plugin.Log.LogWarning("Lobby is modded but does not have any plugin information.");
            return __result;
        }

        return PluginManager.MatchesTargetRequirements(PluginManager.ParseLobbyPluginsMetadata(lobbyPluginString));
    }
}

/// <summary>
///     Patches <see cref="GameNetworkManager.LobbyDataIsJoinable" />.
///     Overrides the joinable to use __joinable instead. This is necessary in case there are required plugins in the
///     lobby, since the API will then disable joining for vanilla clients.
/// </summary>
/// <seealso cref="GameNetworkManager.LobbyDataIsJoinable" />
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class LobbyDataIsJoinableTranspiler
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .SearchBack(instruction => instruction.operand.Equals(LobbyMetadata.Joinable))
            .ThrowIfInvalid("Could not find joinable")
            .RemoveInstruction()
            .Insert(new CodeInstruction(OpCodes.Ldstr, LobbyMetadata.JoinableModded))
            .InstructionEnumeration();
    }
}