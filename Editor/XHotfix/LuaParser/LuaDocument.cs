using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    /// <summary>
    /// Lua文档
    /// </summary>
    public sealed class LuaDocument
    {
        public List<LuaVariable> Variables = new List<LuaVariable>();
        public List<LuaFunction> Functions = new List<LuaFunction>();

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Variables", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUI.enabled = false;
            if (GUILayout.Button("+", "MiniButtonLeft"))
            {

            }
            if (GUILayout.Button("-", "MiniButtonRight"))
            {

            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            for (int i = 0; i < Variables.Count; i++)
            {
                Variables[i].OnGUI();
            }

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Functions", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUI.enabled = false;
            if (GUILayout.Button("+", "MiniButtonLeft"))
            {

            }
            if (GUILayout.Button("-", "MiniButtonRight"))
            {

            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            for (int i = 0; i < Functions.Count; i++)
            {
                Functions[i].OnGUI();
            }
        }
    }
}