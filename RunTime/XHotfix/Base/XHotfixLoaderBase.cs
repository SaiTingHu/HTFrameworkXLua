using System;
using System.Collections.Generic;
using UnityEngine;

namespace HT.Framework.XLua
{
    /// <summary>
    /// XLua加载器基类
    /// </summary>
    public abstract class XHotfixLoaderBase : IDisposable
    {
        protected Dictionary<string, TextAsset> _luaCodes = new Dictionary<string, TextAsset>();
        
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器内加载Lua脚本
        /// </summary>
        /// <param name="assetsPath">Lua脚本路径</param>
        /// <returns>是否加载成功</returns>
        public abstract bool OnLoadLuaCodeEditor(string assetsPath);
#endif
        /// <summary>
        /// 加载Lua脚本
        /// </summary>
        /// <param name="assetBundleName">Lua脚本AB包名称</param>
        public abstract bool OnLoadLuaCode(string assetBundleName);

        public abstract byte[] OnLoadRequire(ref string chunkName);

        public virtual void Dispose()
        {
            _luaCodes.Clear();
        }
    }
}