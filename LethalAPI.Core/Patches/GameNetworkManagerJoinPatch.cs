﻿// -----------------------------------------------------------------------
// <copyright file="GameNetworkManagerJoinPatch.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the GPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace LethalAPI.Core.Patches;

using System.Linq;

using HarmonyLib;
using Steamworks;
using Steamworks.Data;

/// <summary>
/// Patches <see cref="GameNetworkManager.SteamMatchmaking_OnLobbyCreated"/> to add modded flags to the lobby.
/// </summary>
[HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyCreated")]
[HarmonyWrapSafe]
[HarmonyPriority(Priority.Last)]
internal static class SteamMatchmakingOnLobbyCreatedPostfix
{
    [HarmonyPostfix]
    private static void Postfix(Result result, ref Lobby lobby)
    {
        // lobby has not yet created or something went wrong
        if (result != Result.OK)
        {
            return;
        }

        lobby.SetData("__modded_lobby", "true");
        lobby.SetData("__joinable", lobby.GetData("joinable")); // the actual joinable

        // if the user is forced to only allow modded user to join, joinable flag is set to prevent vanilla user to join
        if (ModdedLobbyManager.ModdedOnly)
        {
            lobby.SetData("joinable", "false");
        }
    }
}

/// <summary>
/// Patches the <see cref="GameNetworkManager.LobbyDataIsJoinable"/> to ensure modded players only join.
/// </summary>
[HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
[HarmonyPriority(Priority.Last)]
[HarmonyWrapSafe]
internal static class LobbyDataIsJoinablePrefix
{
    [HarmonyPrefix]
    private static bool Prefix(GameNetworkManager __instance, ref Lobby lobby, ref bool __result)
    {
        Plugin.Log.LogDebug($"Attempting to join lobby id: {lobby.Id}");
        string data = lobby.GetData("__modded_lobby"); // is modded lobby?
        if (ModdedLobbyManager.ModdedOnly && data != "true")
        {
            Plugin.Log.LogDebug("Lobby join denied! Attempted to join non-modded lobby");
            UObject.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, "The server host is not a modded user");
            __result = false;
            return false;
        }

        data = lobby.GetData("vers"); // game version
        if (data != __instance.gameVersionNum.ToString())
        {
            Plugin.Log.LogDebug($"Lobby join denied! Attempted to join vers {data}");
            UObject.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, $"The server host is playing on version {data} while you are on version {GameNetworkManager.Instance.gameVersionNum}.");
            __result = false;
            return false;
        }

        Friend[] friendArr = SteamFriends.GetBlocked().ToArray();

        if (friendArr is { Length: > 0 })
        {
            foreach (Friend friend in friendArr)
            {
                Plugin.Log.LogDebug($"Lobby join denied! Attempted to join a lobby owned by a user which you has blocked: name: {friend.Name} | id: {friend.Id}");
                if (lobby.IsOwnedBy(friend.Id))
                {
                    UObject.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, "You attempted to join a lobby owned by a user you blocked.");
                    __result = false;
                    return false;
                }
            }
        }

        data = lobby.GetData("__joinable"); // is lobby joinable?
        if (data == "false")
        {
            Plugin.Log.LogDebug("Lobby join denied! Host lobby is not joinable");
            UObject.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, "The server host has already landed their ship, or they are still loading in.");
            return false;
        }

        if (lobby.MemberCount is >= 4 or < 1)
        {
            Plugin.Log.LogDebug($"Lobby join denied! Too many members in lobby! {lobby.Id}");
            UObject.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.Full, "The server is full!");
            __result = false;
            return false;
        }

        Plugin.Log.LogDebug($"Lobby join accepted! Lobby ID: {lobby.Id}");
        __result = true;
        return false;
    }
}