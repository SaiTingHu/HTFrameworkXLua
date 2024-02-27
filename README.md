# Unity HTFramework XLua

基于XLua的Unity跨平台热更新模块，必须依赖于HTFramework主框架使用。

## 环境

- Unity版本：2022.3.17。

- .NET API版本：.NET Framework。

- [XLua V2.1.14](https://github.com/Tencent/xLua)。

- [HTFramework(Latest version)](https://github.com/SaiTingHu/HTFramework)。

## 模块简介

- [XHotfix](https://wanderer.blog.csdn.net/article/details/104993852) - 本模块旨在结合XLua与框架的资源加载策略，快速实现热更流程，并优化了开发环境，使得开发人员可以最低成本的投入到Lua业务开发。

## 使用方法

- 1.拉取框架到项目中的Assets文件夹下（Assets/HTFramework/），或以添加子模块的形式。

- 2.在入口场景的层级（Hierarchy）视图点击右键，选择 HTFramework -> Main Environment（创建框架主环境），并删除入口场景其他的东西（除了框架的主要模块，其他任何东西都应该是动态加载的）。

- 3.拉取本模块到项目中的Assets文件夹下（Assets/HTFrameworkXLua/），或以添加子模块的形式。

- 4.在入口场景的层级（Hierarchy）视图点击右键，选择 HTFramework.XLua -> XLua Environment（创建XLua主环境），并一键创建热更新环境。

- 5.参阅各个模块的帮助文档，开始开发。
