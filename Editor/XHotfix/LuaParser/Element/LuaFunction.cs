using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    /// <summary>
    /// Lua·½·¨
    /// </summary>
    public sealed class LuaFunction : LuaElement
    {
        public string Name;
        public string Body;

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.TextArea(Body);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}