using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    [CustomEditor(typeof(TextAsset))]
    internal sealed class LuaInspector : HTFEditor<TextAsset>
    {
        private bool _isLuaScript;
        private LuaParser _luaParser;
        private LuaShowType _showType = LuaShowType.Parser;
        private Vector2 _scrollExplain;
        private Vector2 _scrollScript;
        
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
                _luaParser = new LuaParser(Target.text);
            }
        }
        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            if (_isLuaScript)
            {
                OnTitleGUI();

                if (_showType == LuaShowType.Parser)
                {
                    OnParserGUI();
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
        
        /// <summary>
        /// 标题GUI
        /// </summary>
        private void OnTitleGUI()
        {
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(_showType == LuaShowType.Parser, "Lua Parser", "ButtonLeft", GUILayout.Width(100)))
            {
                _showType = LuaShowType.Parser;
            }
            if (GUILayout.Toggle(_showType == LuaShowType.Script, "Lua Script", "ButtonRight", GUILayout.Width(100)))
            {
                _showType = LuaShowType.Script;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// 解析GUI
        /// </summary>
        private void OnParserGUI()
        {
            GUILayout.BeginVertical();
            _scrollExplain = GUILayout.BeginScrollView(_scrollExplain);

            _luaParser.Document.OnGUI();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        /// <summary>
        /// 脚本GUI
        /// </summary>
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
        
        private enum LuaShowType
        {
            Parser,
            Script
        }
    }
}