using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Extensions
{
    using Structs;

    public static class RoomDataExtensions
    {
        #region RoomInfo Extensions

        public static void SetData<T>(this ref RoomInfo roomInfo, string key, T value)
        {
            roomInfo.CustomData ??= new Dictionary<string, string>();
            roomInfo.CustomData[key] = Serialize(value);
        }

        public static T GetData<T>(this RoomInfo roomInfo, string key)
        {
            if (roomInfo.CustomData == null || !roomInfo.CustomData.TryGetValue(key, out var raw))
                return default;

            try
            {
                return Deserialize<T>(raw);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RoomDataExtensions] Failed to deserialize key '{key}' as {typeof(T).Name}: {e.Message}");
                return default;
            }
        }

        public static bool TryGetData<T>(this RoomInfo roomInfo, string key, out T value)
        {
            value = default;

            if (roomInfo.CustomData == null || !roomInfo.CustomData.TryGetValue(key, out var raw))
                return false;

            try
            {
                value = Deserialize<T>(raw);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region RoomBuilder Extensions

        public static RoomBuilder WithData<T>(this RoomBuilder builder, string key, T value)
        {
            return builder.WithCustomData(key, Serialize(value));
        }

        #endregion

        #region Serialization

        private static string Serialize<T>(T value)
        {
            if (value == null) return string.Empty;

            var type = typeof(T);

            if (type == typeof(string)) return value as string ?? string.Empty;
            if (type == typeof(int)) return ((int)(object)value).ToString(CultureInfo.InvariantCulture);
            if (type == typeof(float)) return ((float)(object)value).ToString(CultureInfo.InvariantCulture);
            if (type == typeof(double)) return ((double)(object)value).ToString(CultureInfo.InvariantCulture);
            if (type == typeof(bool)) return ((bool)(object)value).ToString();
            if (type == typeof(long)) return ((long)(object)value).ToString(CultureInfo.InvariantCulture);

            return JsonUtility.ToJson(value);
        }

        private static T Deserialize<T>(string raw)
        {
            if (raw == null) return default;

            var type = typeof(T);

            if (type == typeof(string)) return (T)(object)raw;
            if (type == typeof(int)) return (T)(object)int.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(float)) return (T)(object)float.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(double)) return (T)(object)double.Parse(raw, CultureInfo.InvariantCulture);
            if (type == typeof(bool)) return (T)(object)bool.Parse(raw);
            if (type == typeof(long)) return (T)(object)long.Parse(raw, CultureInfo.InvariantCulture);

            return JsonUtility.FromJson<T>(raw);
        }

        #endregion
    }
}
