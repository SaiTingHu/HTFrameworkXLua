using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    [CustomEditor(typeof(TextAsset))]
    internal sealed class LuaInspector : HTFEditor<TextAsset>
    {
        private bool _isLuaScript = false;
        private List<LuaVariable> _variables;
        private List<LuaFunction> _functions;
        private LuaShowType _showType = LuaShowType.Explain;
        private Vector2 _scrollExplain;
        private Vector2 _scrollScript;

        #region Keyword
        private readonly string _local = "local";
        private readonly string _function = "function";
        private readonly string _if = "if";
        private readonly string _for = "for";
        private readonly string _while = "while";
        private readonly string _end = "end";
        private readonly string _noteStart = "--[[";
        private readonly string _noteEnd = "--]]";
        private readonly string _note = "--";
        #endregion

        protected override bool IsEnableRuntimeData
        {
            get
            {
                return false;
            }
        }

        protected override bool IsEnableBaseInspectorGUI
        {
            get
            {
                return !_isLuaScript;
            }
        }

        protected override void OnDefaultEnable()
        {
            base.OnDefaultEnable();

            _isLuaScript = Target.IsLuaScript();

            if (_isLuaScript)
            {
                OnLuaExplain();
            }
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            if (_isLuaScript)
            {
                OnTitleGUI();

                if (_showType == LuaShowType.Explain)
                {
                    OnExplainGUI();
                }
                else
                {
                    OnScriptGUI();
                }
            }
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();

            if (_isLuaScript)
            {
                GUI.Label(new Rect(45, 25, 65, 20), "Lua Script", "AssetLabel");
            }
        }

        private void OnLuaExplain()
        {
            _variables = new List<LuaVariable>();
            _functions = new List<LuaFunction>();
            StringBuilder builder = new StringBuilder();
            
            string[] codes = Target.text.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < codes.Length; i++)
            {
                codes[i] = codes[i].Trim();
                if (string.IsNullOrEmpty(codes[i]) || codes[i] == "")
                {
                    continue;
                }
                
                //多行注释
                if (codes[i].StartsWith(_noteStart))
                {
                    if (codes[i].EndsWith(_noteEnd))
                    {
                        continue;
                    }
                    else
                    {
                        int j = i + 1;
                        for (; j < codes.Length; j++)
                        {
                            if (codes[j].StartsWith(_noteEnd))
                            {
                                break;
                            }
                        }
                        i = j;
                        continue;
                    }
                }

                //单行注释
                if (codes[i].StartsWith(_note))
                {
                    continue;
                }

                //标记局部
                bool isLocal = codes[i].StartsWith(_local);
                if (isLocal) codes[i] = codes[i].Remove(0, 5).TrimStart();

                //标记方法
                bool isFunction = codes[i].StartsWith(_function);
                if (isFunction)
                {
                    codes[i] = codes[i].Remove(0, 8).TrimStart();
                    LuaFunction function = new LuaFunction();
                    function.Name = codes[i];
                    if (isLocal) function.Name = "local " + function.Name;
                    int endCount = 0;
                    builder.Clear();
                    int j = i + 1;
                    for (; j < codes.Length; j++)
                    {
                        codes[j] = codes[j].Trim();
                        if (string.IsNullOrEmpty(codes[j]) || codes[j] == "")
                        {
                            continue;
                        }

                        if (codes[j].StartsWith(_end))
                        {
                            if (endCount > 0)
                            {
                                endCount -= 1;
                                builder.Append(codes[j]);
                                builder.Append(Environment.NewLine);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (codes[j].StartsWith(_if) || codes[j].StartsWith(_for) || codes[j].StartsWith(_while))
                            {
                                endCount += 1;
                            }
                            builder.Append(codes[j]);
                            builder.Append(Environment.NewLine);
                        }
                    }
                    i = j;
                    function.Body = builder.ToString().Trim();
                    _functions.Add(function);
                }
                //标记变量
                else
                {
                    string[] keyValue = codes[i].Split('=');
                    if (keyValue.Length == 2)
                    {
                        string[] keys = keyValue[0].Split(',');
                        string[] values = keyValue[1].Split(',');
                        for (int j = 0; j < keys.Length; j++)
                        {
                            LuaVariable variable = new LuaVariable();
                            variable.Name = keys[j];
                            if (isLocal) variable.Name = "local " + variable.Name;
                            variable.Value = j < values.Length ? values[j] : "nil";
                            _variables.Add(variable);
                        }
                    }
                }
            }
        }

        private void OnTitleGUI()
        {
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(_showType == LuaShowType.Explain, "Lua Explain", "ButtonLeft", GUILayout.Width(100)))
            {
                _showType = LuaShowType.Explain;
            }
            if (GUILayout.Toggle(_showType == LuaShowType.Script, "Lua Script", "ButtonRight", GUILayout.Width(100)))
            {
                _showType = LuaShowType.Script;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void OnExplainGUI()
        {
            GUILayout.BeginVertical();
            _scrollExplain = GUILayout.BeginScrollView(_scrollExplain);

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

            for (int i = 0; i < _variables.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(_variables[i].Name, _variables[i].Value);
                GUILayout.EndHorizontal();
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

            for (int i = 0; i < _functions.Count; i++)
            {
                GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);

                GUILayout.BeginHorizontal();
                GUILayout.Label(_functions[i].Name);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextArea(_functions[i].Body);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnScriptGUI()
        {
            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);
            _scrollScript = GUILayout.BeginScrollView(_scrollScript);

            GUILayout.BeginHorizontal();
            GUILayout.Label(Target.text);
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private class LuaVariable
        {
            public string Name;
            public string Value;
        }

        private class LuaFunction
        {
            public string Name;
            public string Body;
        }

        private enum LuaShowType
        {
            Explain,
            Script
        }
    }
}