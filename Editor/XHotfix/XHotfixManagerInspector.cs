using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    [GiteeURL("https://gitee.com/SaiTingHu/HTFrameworkXLua")]
    [CSDNBlogURL("https://wanderer.blog.csdn.net/article/details/104993852")]
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
            string hotfixDirectory = Application.dataPath + Target.HotfixCodeAssetsPath.Replace("Assets", "") + "/";
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

            object value = Target.GetType().GetField("_loader", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Target);
            if (value != null)
            {
                _luaCodes = value.GetType().GetField("_luaCodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value) as Dictionary<string, TextAsset>;
            }
            else
            {
                _luaCodes = new Dictionary<string, TextAsset>();
            }
        }
        protected override void OnInspectorDefaultGUI()
        {
            base.OnInspectorDefaultGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("XHotfix manager, this module encapsulated using XLua!", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");

            #region XHotfixProperty
            GUILayout.BeginHorizontal();
            GUILayout.Label("Loader Type", GUILayout.Width(100));
            if (GUILayout.Button(Target.XHotfixLoaderType, EditorGlobalTools.Styles.MiniPopup))
            {
                GenericMenu gm = new GenericMenu();
                List<Type> types = ReflectionToolkit.GetTypesInRunTimeAssemblies(type =>
                {
                    return type.IsSubclassOf(typeof(XHotfixLoaderBase));
                });
                for (int i = 0; i < types.Count; i++)
                {
                    int j = i;
                    gm.AddItem(new GUIContent(types[j].FullName), Target.XHotfixLoaderType == types[j].FullName, () =>
                    {
                        Undo.RecordObject(target, "Set XHotfixLoader");
                        Target.XHotfixLoaderType = types[j].FullName;
                        HasChanged();
                    });
                }
                gm.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto StartUp", GUILayout.Width(100));
            Toggle(Target.IsAutoStartUp, out Target.IsAutoStartUp, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tick Interval", GUILayout.Width(100));
            FloatField(Target.TickInterval, out Target.TickInterval, "");
            GUILayout.EndHorizontal();
            #endregion
            
            GUILayout.Space(10);

            #region XHotfixAssetBundle
            GUILayout.BeginHorizontal();
            GUILayout.Label("XHotfixCode AssetBundleName");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            TextField(Target.HotfixCodeAssetBundleName, out Target.HotfixCodeAssetBundleName, "");
            GUI.enabled = _hotfixIsCreated;
            if (GUILayout.Button("Mark All", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                MarkLuaFolder(new DirectoryInfo(Application.dataPath + Target.HotfixCodeAssetsPath.Replace("Assets", "") + "/"));
            }
            GUI.enabled = true;
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
            #endregion

            GUILayout.EndVertical();

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
            GUILayout.Label("Loaded Lua Codes: ", GUILayout.Width(160));
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
            string hotfixDirectory = Application.dataPath + Target.HotfixCodeAssetsPath.Replace("Assets", "") + "/";
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
                File.AppendAllText(filePath, code, Encoding.UTF8);
                asset = null;
                AssetDatabase.Refresh();
            }
        }
        private void MarkLuaFolder(DirectoryInfo directoryInfo)
        {
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
            for (int i = 0; i < fileSystemInfos.Length; i++)
            {
                if (fileSystemInfos[i] is FileInfo)
                {
                    MarkLuaScript(fileSystemInfos[i] as FileInfo);
                }
                else if (fileSystemInfos[i] is DirectoryInfo)
                {
                    MarkLuaFolder(fileSystemInfos[i] as DirectoryInfo);
                }
            }
        }
        private void MarkLuaScript(FileInfo fileInfo)
        {
            if (fileInfo.FullName.EndsWith(".lua.txt"))
            {
                string path = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf("Assets"));
                AssetImporter importer = AssetImporter.GetAtPath(path);
                importer.assetBundleName = Target.HotfixCodeAssetBundleName;
                importer.SaveAndReimport();
                Log.Info("已标记 " + Target.HotfixCodeAssetBundleName + " 于 " + path);
            }
        }
    }
}