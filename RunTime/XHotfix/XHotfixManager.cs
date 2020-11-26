using System;
using UnityEngine;
using XLua;

namespace HT.Framework.XLua
{
    /// <summary>
    /// XLua热更新模块管理者
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    public sealed class XHotfixManager : HTBehaviour
    {
        public static XHotfixManager Current;

        /// <summary>
        /// 当前的XLua加载器类型
        /// </summary>
        public string XHotfixLoaderType = "HT.Framework.XLua.XHotfixDefaultLoader";
        /// <summary>
        /// 自动启动
        /// </summary>
        public bool IsAutoStartUp = true;
        /// <summary>
        /// 热更新代码文件AB包名称
        /// </summary>
        public string HotfixCodeAssetBundleName = "xhotfix";
        /// <summary>
        /// 热更新代码文件主路径
        /// </summary>
        public string HotfixCodeAssetsPath = "Assets/XHotfix";
        /// <summary>
        /// 热更新代码主模块
        /// </summary>
        public string HotfixCodeMain = "Main";
        /// <summary>
        /// XLua的Tick间隔时间
        /// </summary>
        public float TickInterval = 1;

        private bool _isStartUp = false;
        private float _lastGCTime = 0;
        private float _timer = 0;
        private XHotfixLoaderBase _loader;
        private LuaEnv _luaEnv;
        private LuaTable _luaTable;
        private Action _luaOnInitialization;
        private Action _luaOnPreparatory;
        private Action _luaOnRefresh;
        private Action _luaOnRefreshSecond;
        private Action _luaOnTermination;

        protected override void Awake()
        {
            base.Awake();

            Current = this;

            Type type = ReflectionToolkit.GetTypeInRunTimeAssemblies(XHotfixLoaderType);
            if (type != null)
            {
                if (type.IsSubclassOf(typeof(XHotfixLoaderBase)))
                {
                    _loader = Activator.CreateInstance(type) as XHotfixLoaderBase;
                }
                else
                {
                    Log.Error("创建XLua加载器失败：XLua加载器类 " + XHotfixLoaderType + " 必须继承至加载器基类：XHotfixLoaderBase！");
                }
            }
            else
            {
                Log.Error("创建XLua加载器失败：丢失XLua加载器类 " + XHotfixLoaderType + "！");
            }

            _luaEnv = new LuaEnv();
            _luaEnv.AddLoader(_loader.OnLoadRequire);
            _luaTable = _luaEnv.NewTable();
            
            LuaTable meta = _luaEnv.NewTable();
            meta.Set("__index", _luaEnv.Global);
            _luaTable.SetMetaTable(meta);
            meta.Dispose();

            if (Main.m_Resource.LoadMode == ResourceLoadMode.Resource)
            {
                Log.Error("热更新初始化失败：热更新代码不支持使用Resource加载模式！");
                return;
            }
        }

        private void Start()
        {
            if (IsAutoStartUp)
            {
                StartUp();
            }
        }

        private void Update()
        {
            _luaOnRefresh?.Invoke();

            if (_timer < 1)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                _timer = 0;
                _luaOnRefreshSecond?.Invoke();
            }

            if (Time.time - _lastGCTime > TickInterval)
            {
                _luaEnv.Tick();
                _lastGCTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            _luaOnTermination?.Invoke();

            _luaOnInitialization = null;
            _luaOnPreparatory = null;
            _luaOnRefresh = null;
            _luaOnRefreshSecond = null;
            _luaOnTermination = null;

            _loader.Dispose();
            _loader = null;

            _luaTable.Dispose();
            _luaTable = null;
            _luaEnv = null;
        }

        /// <summary>
        /// Lua虚拟机
        /// </summary>
        public LuaEnv LuaVM
        {
            get
            {
                return _luaEnv;
            }
        }

        /// <summary>
        /// Lua全局环境
        /// </summary>
        public LuaTable LuaGlobal
        {
            get
            {
                return _luaEnv.Global;
            }
        }

        /// <summary>
        /// 启动热更新
        /// </summary>
        public void StartUp()
        {
            if (!_isStartUp)
            {
                _isStartUp = true;
                AssetInfo codeInfo = new AssetInfo(HotfixCodeAssetBundleName, HotfixCodeAssetsPath + "/" + HotfixCodeMain + ".lua.txt", "");
                Main.m_Resource.LoadAsset<TextAsset>(codeInfo, null, HotfixCodeMainLoadDone);
            }
        }

        /// <summary>
        /// 获取Lua全局对象（字段、表、方法）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">名称</param>
        /// <returns>对象</returns>
        public T Get<T>(string key)
        {
            return LuaGlobal.Get<T>(key);
        }

        /// <summary>
        /// 设置Lua全局对象
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void Set<TKey, TValue>(TKey key, TValue value)
        {
            LuaGlobal.Set(key, value);
        }

        private void HotfixCodeMainLoadDone(TextAsset asset)
        {
            if (asset)
            {
#if UNITY_EDITOR
                _isStartUp = _loader.OnLoadLuaCodeEditor(HotfixCodeAssetsPath);
                if (_isStartUp)
                {
                    StartUpXLua();
                }
#else
                _isStartUp = _loader.OnLoadLuaCode(HotfixCodeAssetBundleName);
                if (_isStartUp)
                {
                    StartUpXLua();
                }
#endif
            }
            else
            {
                _isStartUp = false;
                Log.Error("热更新初始化失败：未拉取到热更新代码主模块 " + HotfixCodeMain + "！");
            }
        }

        private void StartUpXLua()
        {
            _luaEnv.DoString("require '" + HotfixCodeMain + "'", HotfixCodeMain, _luaTable);
            
            _luaOnInitialization = LuaGlobal.Get<Action>("OnInitialization");
            _luaOnPreparatory = LuaGlobal.Get<Action>("OnPreparatory");
            _luaOnRefresh = LuaGlobal.Get<Action>("OnRefresh");
            _luaOnRefreshSecond = LuaGlobal.Get<Action>("OnRefreshSecond");
            _luaOnTermination = LuaGlobal.Get<Action>("OnTermination");

            _luaOnInitialization?.Invoke();
            _luaOnPreparatory?.Invoke();
        }
    }
}