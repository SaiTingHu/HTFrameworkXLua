using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    /// <summary>
    /// Lua±‰¡ø
    /// </summary>
    public sealed class LuaVariable : LuaElement
    {
        public string Name;
        public string Value;

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField(Name, Value);
            GUILayout.EndHorizontal();
        }
    }
}