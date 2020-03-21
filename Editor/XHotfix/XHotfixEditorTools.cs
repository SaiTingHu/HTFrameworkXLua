using CSObjectWrapEditor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HT.Framework.XLua
{
    /// <summary>
    /// 编辑器工具
    /// </summary>
    public static class XHotfixEditorTools
    {
        #region GenPath
        [GenPath]
        private static string Common_path = Application.dataPath + "/HTFrameworkXLua/XLua/Gen/";
        #endregion

        #region XLua Menu
        [MenuItem("XLua/Import XLua Tools")]
        private static void ImportXLuaTools()
        {
            string path = GlobalTools.GetDirectorySameLevelOfAssets("/Tools");
            if (Directory.Exists(path))
            {
                GlobalTools.LogWarning("已存在 XLua Tools：" + path);
                return;
            }

            ZipFile.DeCompress(Application.dataPath + "/HTFrameworkXLua/Editor/XLua/Tools.zip", GlobalTools.GetDirectorySameLevelOfAssets(""), "", true);
            GlobalTools.LogInfo("已成功导入 XLua Tools：" + path);
        }

        [MenuItem("XLua/Import XLua WebGLPlugins")]
        private static void ImportXLuaWebGLPlugins()
        {
            string path = GlobalTools.GetDirectorySameLevelOfAssets("/WebGLPlugins");
            if (Directory.Exists(path))
            {
                GlobalTools.LogWarning("已存在 XLua WebGLPlugins：" + path);
                return;
            }

            ZipFile.DeCompress(Application.dataPath + "/HTFrameworkXLua/Editor/XLua/WebGLPlugins.zip", GlobalTools.GetDirectorySameLevelOfAssets(""), "", true);
            GlobalTools.LogInfo("已成功导入 XLua WebGLPlugins：" + path);
        }
        #endregion

        #region 工程视图新建菜单
        /// <summary>
        /// 【验证函数】新建XHotfix的Lua脚本
        /// </summary>
        [@MenuItem("Assets/Create/HTFramework XLua/[XHotfix] Lua Script", true)]
        private static bool CreateXHotfixLuaValidate()
        {
            return AssetDatabase.IsValidFolder("Assets/XHotfix");
        }

        /// <summary>
        /// 新建XHotfix的Lua脚本
        /// </summary>
        [@MenuItem("Assets/Create/HTFramework XLua/[XHotfix] Lua Script", false, 0)]
        private static void CreateXHotfixLua()
        {
            string path = EditorUtility.SaveFilePanel("新建 Lua 类", Application.dataPath + "/XHotfix", "NewLua", "lua");
            if (path != "")
            {
                string className = path.Substring(path.LastIndexOf("/") + 1).Replace(".lua", "");
                path += ".txt";
                if (!File.Exists(path))
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath("Assets/HTFrameworkXLua/Editor/XHotfix/Template/LuaTemplate.txt", typeof(TextAsset)) as TextAsset;
                    if (asset)
                    {
                        string code = asset.text;
                        code = code.Replace("#MODULENAME#", className);
                        File.AppendAllText(path, code);
                        asset = null;
                        AssetDatabase.Refresh();

                        string assetPath = path.Substring(path.LastIndexOf("Assets"));
                        TextAsset lua = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset)) as TextAsset;
                        EditorGUIUtility.PingObject(lua);
                        Selection.activeObject = lua;
                        AssetDatabase.OpenAsset(lua);
                    }
                }
                else
                {
                    GlobalTools.LogError("新建Lua失败，已存在同名脚本 " + className);
                }
            }
        }
        #endregion

        #region 扩展工具
        /// <summary>
        /// 是否是Lua脚本
        /// </summary>
        /// <param name="textAsset">文本文件</param>
        /// <returns>是否是Lua脚本</returns>
        public static bool IsLuaScript(this TextAsset textAsset)
        {
            return textAsset.name.EndsWith(".lua");
        }
        #endregion
    }
}