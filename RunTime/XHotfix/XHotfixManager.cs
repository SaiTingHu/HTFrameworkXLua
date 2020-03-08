using System.Text;
using UnityEngine;
using XLua;

namespace HT.Framework.XLua
{
    /// <summary>
    /// XLua热更新模块管理者
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    public sealed class XHotfixManager : MonoBehaviour
    {
        public static XHotfixManager Current;

        private LuaEnv _luaEnv = new LuaEnv();
        private LuaTable _luaTable;
        private float _lastGCTime = 0;
        private float _GCInterval = 1;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Current = this;

            _luaTable = _luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = _luaEnv.NewTable();
            meta.Set("__index", _luaEnv.Global);
            _luaTable.SetMetaTable(meta);
            meta.Dispose();
        }

        private void Update()
        {
            if (Time.time - _lastGCTime > _GCInterval)
            {
                _luaEnv.Tick();
                _lastGCTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            _luaEnv.Dispose();
            _luaEnv = null;
            _luaTable.Dispose();
            _luaTable = null;
        }

        /// <summary>
        /// 执行Lua代码
        /// </summary>
        /// <param name="luaCode">Lua代码字符串</param>
        /// <param name="chunkName">发生error时的debug显示信息中使用，指明某某代码块的某行错误</param>
        /// <param name="env">环境变量</param>
        /// <returns>Lua代码的返回值</returns>
        public object[] DoString(string luaCode, string chunkName = "chunk", LuaTable env = null)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(luaCode);
            return _luaEnv.DoString(bytes, chunkName, env);
        }
    }
}