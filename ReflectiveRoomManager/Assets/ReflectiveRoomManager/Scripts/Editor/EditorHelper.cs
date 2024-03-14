using UnityEditor;
using System.Collections.Generic;

namespace REFLECTIVE.Editor
{
    public static class EditorHelper
    {
        public static IEnumerable<string> IterateOverProject(string filter)
        {
            foreach (string guid in AssetDatabase.FindAssets(filter))
            {
                yield return AssetDatabase.GUIDToAssetPath(guid);
            }
        }
    }
}