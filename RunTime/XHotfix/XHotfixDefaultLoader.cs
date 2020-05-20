using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HT.Framework.XLua
{
    /// <summary>
    /// XLua默认加载器
    /// </summary>
    public sealed class XHotfixDefaultLoader : XHotfixLoaderBase
    {
#if UNITY_EDITOR
        public override bool OnLoadLuaCodeEditor(string assetsPath)
        {
            _luaCodes.Clear();
            DefaultAsset defaultAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetsPath);
            if (defaultAsset)
            {
                Selection.activeObject = defaultAsset;
                TextAsset[] textAssets = Selection.GetFiltered<TextAsset>(SelectionMode.DeepAssets);
                Selection.activeObject = null;
                for (int i = 0; i < textAssets.Length; i++)
                {
                    if (textAssets[i].name.EndsWith(".lua"))
                    {
                        if (_luaCodes.ContainsKey(textAssets[i].name))
                        {
                            string path1 = AssetDatabase.GetAssetPath(_luaCodes[textAssets[i].name]);
                            string path2 = AssetDatabase.GetAssetPath(textAssets[i]);
                            Log.Error("加载Lua脚本失败：发现同名脚本 " + path1 + " 和 " + path2);
                        }
                        else
                        {
                            _luaCodes.Add(textAssets[i].name, textAssets[i]);
                        }
                    }
                }
                return true;
            }
            else
            {
                Log.Error("热更新初始化失败：当前未创建热更新环境！");
                return false;
            }
        }
#endif
        public override bool OnLoadLuaCode(string assetBundleName)
        {
            AssetBundle assetBundle = Main.m_Resource.GetAssetBundle(assetBundleName);
            if (assetBundle)
            {
                TextAsset[] textAssets = assetBundle.LoadAllAssets<TextAsset>();
                for (int i = 0; i < textAssets.Length; i++)
                {
                    if (textAssets[i].name.EndsWith(".lua"))
                    {
                        if (_luaCodes.ContainsKey(textAssets[i].name))
                        {
                            Log.Error("加载Lua脚本失败：发现同名脚本 " + textAssets[i].name);
                        }
                        else
                        {
                            _luaCodes.Add(textAssets[i].name, textAssets[i]);
                        }
                    }
                }
                return true;
            }
            else
            {
                Log.Error("热更新初始化失败：未拉取到热更新代码所属的AB包，或已提前释放该资源包！");
                return false;
            }
        }
        
        public override byte[] OnLoadRequire(ref string chunkName)
        {
            string fileName = chunkName + ".lua";
            if (_luaCodes.ContainsKey(fileName))
            {
                return _luaCodes[fileName].bytes;
            }
            else
            {
                return null;
            }
        }
    }
}