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
    }
}