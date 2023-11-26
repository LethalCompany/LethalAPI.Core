﻿// -----------------------------------------------------------------------
// <copyright file="Vector3Converter.cs" company="LethalAPI Modding Community">
// Copyright (c) LethalAPI Modding Community. All rights reserved.
// Licensed under the GPL-3.0 license.
// </copyright>
// From https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters
// Licensed under MIT.
// View the license here:
// https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters/blob/master/LICENSE.md
// -----------------------------------------------------------------------

namespace LethalAPI.Core.Loader.Configs.Converters.Json;

using Helpers;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Custom Newtonsoft.Json converter <see cref="JsonConverter{T}"/> for the Unity Vector3 type <see cref="Vector3"/>.
/// </summary>
public class Vector3Converter : PartialConverter<Vector3>
{
    /// <inheritdoc />
    protected override void ReadValue(ref Vector3 value, string name, JsonReader reader, JsonSerializer serializer)
    {
        switch (name)
        {
            case nameof(value.x):
                value.x = reader.ReadAsFloat() ?? 0f;
                break;
            case nameof(value.y):
                value.y = reader.ReadAsFloat() ?? 0f;
                break;
            case nameof(value.z):
                value.z = reader.ReadAsFloat() ?? 0f;
                break;
        }
    }

    /// <inheritdoc />
    protected override void WriteJsonProperties(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WritePropertyName(nameof(value.x));
        writer.WriteValue(value.x);
        writer.WritePropertyName(nameof(value.y));
        writer.WriteValue(value.y);
        writer.WritePropertyName(nameof(value.z));
        writer.WriteValue(value.z);
    }
}