# Unity HTFramework XLua

基于XLua的Unity跨平台热更新模块，必须依赖于HTFramework主框架使用。

## 环境

- Unity版本：2018.3.0及以上。

- .NET API版本：4.x。

- [XLua V2.1.14](https://github.com/Tencent/xLua)。

- [HTFramework](https://github.com/SaiTingHu/HTFramework)。

## 模块简介

## 使用方法

- 1.拉取框架到项目中的Assets文件夹下（Assets/HTFramework/）。

- 2.将框架根目录下的HTFramework.prefab拖到主场景，并删除主场景其他的东西（除了框架的主要模块，其他任何东西都应该是动态加载的）。

- 3.拉取本模块到项目中的Assets文件夹下（Assets/HTFrameworkXLua/）。

- 4.将HTFrameworkXLua目录下的HTFrameworkXLua.prefab拖到主场景，并一键创建热更新环境。

- 5.开始开发。
