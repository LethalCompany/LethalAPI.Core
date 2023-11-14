﻿// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="Lethal Company Modding Community">
// Copyright (c) Lethal Company Modding Community. All rights reserved.
// Licensed under the GPL-3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace LCAPI.Core;

using BepInEx;
using BepInEx.Logging;

/// <inheritdoc />
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    /// <summary>
    /// Gets the singleton for a plugin.
    /// </summary>
#pragma warning disable SA1401
    public static Plugin Singleton;
#pragma warning restore SA1401

    /// <summary>
    /// Gets the <see cref="Logger"/>.
    /// </summary>
    public ManualLogSource Log => this.Logger;

    private void Awake()
    {
        Singleton = this;

        // Plugin startup logic
        this.Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }
}