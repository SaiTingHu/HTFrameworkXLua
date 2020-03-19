using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        /// <summary>
        /// 自动启动
        /// </summary>
        public bool IsAutoStartUp = false;
        /// <summary>
        /// 热更新代码文件AB包名称
        /// </summary>
        public string HotfixCodeAssetBundleName = "xhotfix";
        /// <summary>
        /// 热更新代码文件主路径
        /// </summary>
        public string HotfixCodeAssetsPath = "Assets/XHotfix/";
        /// <summary>
        /// 热更新代码主模块
        /// </summary>
        public string HotfixCodeMain = "Main";
        /// <summary>
        /// XLua的Tick间隔时间
        /// </summary>
        public float TickInterval = 1;

        private bool _isStartUp = false;
        private LuaEnv _luaEnv;
        private LuaTable _luaTable;
        private float _lastGCTime = 0;
        private Action _luaOnInitialization;
        private Action _luaOnPreparatory;
        private Action _luaOnRefresh;
        private Action _luaOnTermination;
        private Dictionary<string, TextAsset> _luaCodes = new Dictionary<string, TextAsset>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Current = this;

            _luaEnv = new LuaEnv();
            _luaEnv.AddLoader(XHotfixLoader);
            _luaTable = _luaEnv.NewTable();
            
            LuaTable meta = _luaEnv.NewTable();
            meta.Set("__index", _luaEnv.Global);
            _luaTable.SetMetaTable(meta);
            meta.Dispose();

            if (Main.m_Resource.LoadMode == ResourceLoadMode.Resource)
            {
                GlobalTools.LogError("热更新初始化失败：热更新代码不支持使用Resource加载模式！");
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

            if (Time.time - _lastGCTime > TickInterval)
            {
                _luaEnv.Tick();
                _lastGCTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            _luaOnTermination?.Invoke();

            _luaTable.Dispose();
            _luaTable = null;
            _luaEnv = null;
        }

        /// <summary>
        /// 启动热更新
        /// </summary>
        public void StartUp()
        {
            if (!_isStartUp)
            {
                _isStartUp = true;
                AssetInfo codeInfo = new AssetInfo(HotfixCodeAssetBundleName, HotfixCodeAssetsPath + HotfixCodeMain + ".lua.txt", "");
                Main.m_Resource.LoadAsset<TextAsset>(codeInfo, null, HotfixCodeMainLoadDone);
            }
        }

        private void HotfixCodeMainLoadDone(TextAsset asset)
        {
            if (asset)
            {
                _isStartUp = true;
                _luaCodes.Clear();

#if UNITY_EDITOR
                string assetsPath = HotfixCodeAssetsPath;
                if (assetsPath.EndsWith("/"))
                {
                    assetsPath = assetsPath.Substring(0, assetsPath.LastIndexOf("/"));
                }
                DefaultAsset defaultAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(assetsPath);
                if (defaultAsset)
                {
                    Selection.activeObject = defaultAsset;
                    TextAsset[] textAssets = Selection.GetFiltered<TextAsset>(SelectionMode.DeepAssets);
                    Selection.activeObject = null;
                    for (int i = 0; i < textAssets.Length; i++)
                    {
                        _luaCodes.Add(textAssets[i].name, textAssets[i]);
                    }

                    StartUpXLua();
                }
                else
                {
                    _isStartUp = false;
                    GlobalTools.LogError("热更新初始化失败：当前未创建热更新环境！");
                }
#else
                AssetBundle assetBundle = Main.m_Resource.GetAssetBundle(HotfixCodeAssetBundleName);
                if (assetBundle)
                {
                    TextAsset[] textAssets = assetBundle.LoadAllAssets<TextAsset>();
                    for (int i = 0; i < textAssets.Length; i++)
                    {
                        _luaCodes.Add(textAssets[i].name, textAssets[i]);
                    }

                    StartUpXLua();
                }
                else
                {
                    _isStartUp = false;
                    GlobalTools.LogError("热更新初始化失败：未拉取到热更新代码所属的AB包，或已提前释放该资源包！");
                }
#endif
            }
            else
            {
                _isStartUp = false;
                GlobalTools.LogError("热更新初始化失败：未拉取到热更新代码主模块 " + HotfixCodeMain + "！");
            }
        }

        private void StartUpXLua()
        {
            _luaEnv.DoString("require '" + HotfixCodeMain + "'", HotfixCodeMain, _luaTable);
            
            _luaOnInitialization = _luaTable.Get<Action>("OnInitialization");
            _luaOnPreparatory = _luaTable.Get<Action>("OnPreparatory");
            _luaOnRefresh = _luaTable.Get<Action>("OnRefresh");
            _luaOnTermination = _luaTable.Get<Action>("OnTermination");

            _luaOnInitialization?.Invoke();
            _luaOnPreparatory?.Invoke();
        }

        private byte[] XHotfixLoader(ref string chunkName)
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