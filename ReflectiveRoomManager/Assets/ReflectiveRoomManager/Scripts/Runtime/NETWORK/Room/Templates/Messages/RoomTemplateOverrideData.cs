using System;
using System.Collections.Generic;
using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Network-serializable override data.
    /// </summary>
    [Serializable]
    public struct RoomTemplateOverrideData
    {
        public string RoomName;
        public int MaxPlayers;
        public bool IsPrivate;
        public bool OverridePrivacy;
        public CustomDataEntry[] CustomDataOverride;

        /// <summary>
        /// Converts to RoomTemplateOverride struct.
        /// </summary>
        public RoomTemplateOverride ToRoomTemplateOverride()
        {
            return new RoomTemplateOverride
            {
                RoomName = string.IsNullOrEmpty(RoomName) ? null : RoomName,
                MaxPlayers = MaxPlayers == -1 ? null : (int?)MaxPlayers,
                IsPrivate = OverridePrivacy ? (bool?)IsPrivate : null,
                CustomDataOverride = CustomDataEntryArrayToDictionary(CustomDataOverride)
            };
        }

        /// <summary>
        /// Creates from a RoomTemplateOverride struct.
        /// </summary>
        public static RoomTemplateOverrideData FromRoomTemplateOverride(RoomTemplateOverride templateOverride)
        {
            return new RoomTemplateOverrideData
            {
                RoomName = templateOverride.RoomName ?? string.Empty,
                MaxPlayers = templateOverride.MaxPlayers ?? -1,
                IsPrivate = templateOverride.IsPrivate ?? false,
                OverridePrivacy = templateOverride.IsPrivate.HasValue,
                CustomDataOverride = DictionaryToCustomDataEntryArray(templateOverride.CustomDataOverride)
            };
        }

        private static Dictionary<string, string> CustomDataEntryArrayToDictionary(CustomDataEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return null;

            var result = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    result[entry.Key] = entry.Value ?? string.Empty;
                }
            }
            return result.Count > 0 ? result : null;
        }

        private static CustomDataEntry[] DictionaryToCustomDataEntryArray(Dictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                return Array.Empty<CustomDataEntry>();

            var entries = new CustomDataEntry[dictionary.Count];
            int i = 0;
            foreach (var kvp in dictionary)
            {
                entries[i] = new CustomDataEntry { Key = kvp.Key, Value = kvp.Value };
                i++;
            }
            return entries;
        }
    }

    /// <summary>
    /// Mirror custom reader/writer for RoomTemplateOverrideData.
    /// </summary>
    public static class RoomTemplateOverrideDataReadWrite
    {
        public static void WriteRoomTemplateOverrideData(this NetworkWriter writer, RoomTemplateOverrideData data)
        {
            writer.WriteString(data.RoomName ?? string.Empty);
            writer.WriteInt(data.MaxPlayers);
            writer.WriteBool(data.IsPrivate);
            writer.WriteBool(data.OverridePrivacy);

            // Write custom data array
            if (data.CustomDataOverride == null || data.CustomDataOverride.Length == 0)
            {
                writer.WriteInt(0);
            }
            else
            {
                writer.WriteInt(data.CustomDataOverride.Length);
                foreach (var entry in data.CustomDataOverride)
                {
                    writer.WriteString(entry.Key ?? string.Empty);
                    writer.WriteString(entry.Value ?? string.Empty);
                }
            }
        }

        public static RoomTemplateOverrideData ReadRoomTemplateOverrideData(this NetworkReader reader)
        {
            var data = new RoomTemplateOverrideData
            {
                RoomName = reader.ReadString(),
                MaxPlayers = reader.ReadInt(),
                IsPrivate = reader.ReadBool(),
                OverridePrivacy = reader.ReadBool()
            };

            int count = reader.ReadInt();
            if (count > 0)
            {
                data.CustomDataOverride = new CustomDataEntry[count];
                for (int i = 0; i < count; i++)
                {
                    data.CustomDataOverride[i] = new CustomDataEntry
                    {
                        Key = reader.ReadString(),
                        Value = reader.ReadString()
                    };
                }
            }
            else
            {
                data.CustomDataOverride = Array.Empty<CustomDataEntry>();
            }

            return data;
        }
    }
}
