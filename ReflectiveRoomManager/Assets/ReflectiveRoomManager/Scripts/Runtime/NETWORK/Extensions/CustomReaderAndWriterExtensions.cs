using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Extensions
{
    public static class CustomReaderAndWriterExtensions
    {
        public static void WriteDictionary(this NetworkWriter writer, Dictionary<string, string> dictionary)
        {
            List<string> keys = null;
            List<string> values = null;

            if (dictionary != null)
            {
                keys = dictionary.Keys.ToList();
                values = dictionary.Values.ToList();
            }

            writer.WriteList(keys);
            writer.WriteList(values);
        }

        public static Dictionary<string, string> ReadDictionary(this NetworkReader reader)
        {
            var keys = reader.ReadList<string>();
            var values = reader.ReadList<string>();

            var dict = new Dictionary<string, string>();

            if (keys == null || values == null) return dict;

            for (var i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }

            return dict;
        }
    }
}