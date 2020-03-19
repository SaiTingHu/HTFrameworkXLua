using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    [CSDNBlogURL("")]
    [GithubURL("https://github.com/SaiTingHu/HTFrameworkXLua")]
    [CustomEditor(typeof(XHotfixManager))]
    internal sealed class XHotfixManagerInspector : HTFEditor<XHotfixManager>
    {
        private bool _hotfixIsCreated = false;
        private Dictionary<string, TextAsset> _luaCodes;

        protected override void OnDefaultEnable()
        {
            base.OnDefaultEnable();

            _hotfixIsCreated = false;
            string hotfixDirectory = Application.dataPath + Target.HotfixCodeAssetsPath.Replace("Assets", "");
            string hotfixMainPath = hotfixDirectory + Target.HotfixCodeMain + ".lua.txt";
            if (Directory.Exists(hotfixDirectory))
            {
                if (File.Exists(hotfixMainPath))
                {
                    _hotfixIsCreated = true;
                }
            }
        }

        protected override void OnRuntimeEnable()
        {
            base.OnRuntimeEnable();

            _luaCodes = Target.GetType().GetField("_luaCodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Target) as Dictionary<string, TextAsset>;
        }

        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("XHotfix manager, this module encapsulated using XLua!", MessageType.Info);
            GUILayout.EndHorizontal();

            #region XHotfixCode
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            Toggle(Target.IsAutoStartUp, out Target.IsAutoStartUp, "Auto StartUp");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            FloatField(Target.TickInterval, out Target.TickInterval, "Tick Interval");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("XHotfixCode AssetBundleName");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextField(Target.HotfixCodeAssetBundleName, out Target.HotfixCodeAssetBundleName, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("XHotfixCode AssetsPath");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextField(Target.HotfixCodeAssetsPath, out Target.HotfixCodeAssetsPath, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("XHotfixCode Main");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextField(Target.HotfixCodeMain, out Target.HotfixCodeMain, "");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            #endregion

            #region XHotfixWizard
            if (_hotfixIsCreated)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("XHotfix environment is created!", MessageType.Info);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create XHotfix Environment", "LargeButton"))
                {
                    CreateXHotfixEnvironment();
                    _hotfixIsCreated = true;
                }
                GUILayout.EndHorizontal();
            }
            #endregion
        }

        protected override void OnInspectorRuntimeGUI()
        {
            base.OnInspectorRuntimeGUI();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Lua Codes: ", GUILayout.Width(100));
            GUILayout.Label(_luaCodes.Count.ToString());
            GUILayout.EndHorizontal();

            foreach (KeyValuePair<string, TextAsset> item in _luaCodes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(item.Key);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.ObjectField(item.Value, typeof(TextAsset), true);
                GUILayout.EndHorizontal();
            }
        }

        private void CreateXHotfixEnvironment()
        {
            string hotfixDirectory = Application.dataPath + Target.HotfixCodeAssetsPath.Replace("Assets", "");
            string hotfixMainPath = hotfixDirectory + Target.HotfixCodeMain + ".lua.txt";
            if (!Directory.Exists(hotfixDirectory))
            {
                Directory.CreateDirectory(hotfixDirectory);
            }
            if (!File.Exists(hotfixMainPath))
            {
                CreateXHotfixMain(hotfixMainPath);
            }
        }
        private void CreateXHotfixMain(string filePath)
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath("Assets/HTFrameworkXLua/Editor/XHotfix/Template/MainTemplate.txt", typeof(TextAsset)) as TextAsset;
            if (asset)
            {
                string code = asset.text;
                File.AppendAllText(filePath, code);
                asset = null;
                AssetDatabase.Refresh();
            }
        }
    }
}