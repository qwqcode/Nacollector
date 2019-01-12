<p align="center"><img src="./docs/NacollectorLogo_2.png"></p>

# Nacollector

[![](https://img.shields.io/github/release/qwqcode/Nacollector.svg?style=flat-square)](https://github.com/qwqcode/Nacollector/releases/latest) ![](https://img.shields.io/badge/NET-%3E%3D%204.6.2-green.svg?style=flat-square) [![](https://img.shields.io/github/downloads/qwqcode/Nacollector/total.svg?style=flat-square)](https://github.com/qwqcode/Nacollector/releases) [![](https://img.shields.io/github/last-commit/qwqcode/Nacollector.svg?style=flat-square)](https://github.com/qwqcode/Nacollector/commits) [![](https://img.shields.io/github/issues/qwqcode/Nacollector.svg?style=flat-square)](https://github.com/qwqcode/Nacollector/issues) [![](https://img.shields.io/github/issues-pr/qwqcode/Nacollector.svg?style=flat-square)](https://github.com/qwqcode/Nacollector/pulls) [![](https://img.shields.io/gitter/room/qwqcode/Nacollector.svg?style=flat-square)](https://gitter.im/Nacollector/community) [![](https://img.shields.io/badge/%24-donate-ff69b4.svg?style=flat-square)](https://github.com/qwqcode/donate-qwqaq)

> Nacollector 可以说是一个用于采集各种 WEB 资源的工作站？！ #(滑稽)

注：在使用 Nacollector 前，请仔细阅读[《Nacollector 用户使用许可协议》](./LICENSE)

#### 特性
- Material Design
- [CefSharp](https://github.com/cefsharp/CefSharp) 嵌入 Chromium，用 HTML/CSS/JS 制作前端 Ui
- 前后端分离，[NacollectorFrontend](https://github.com/qwqcode/NacollectorFrontend)
- 前端 Console 实时采集日志
- 多任务管理器，多个采集任务同时进行
- 下载内容管理器，具有和 Chrome 一样的功能
- Cookie 获取器（可手动导入 Cookie，自动填充，Cookie 记录，正则表达式配置规则）
- 多线程异步采集实例
- 资源快速预览
- 支持使用代理
- 在线/离线 自动更新
- 以及更多...

#### Features
- Material Design
- [CefSharp](https://github.com/cefsharp/CefSharp) embed Chromium in the .NET app to use JS/HTML/CSS as Front-end UI
- Separate Front-end and Back-end, [see NacollectorFrontend](https://github.com/qwqcode/NacollectorFrontend)
- Real-time collection logs in the Front-end Console
- Multitasking Manager to manage multiple collection tasks
- Download Content Manager like chrome browser
- Cookie getter (manually import cookies, input auto-complete, keep cookies fresh longer, using regular expressions)
- Multiple Async Tasks example
- Resources Preview
- Support for using proxy
- Automatically update online/offline
- And more...

#### Requirements
- NET >= 4.6.2
- [CefSharp](https://github.com/cefsharp/CefSharp)
- [CsQuery](https://github.com/jamietre/CsQuery)
- Selenium.WebDriver

#### 功能

- 商品详情页图片解析
  - 支持网站： 淘宝、天猫、苏宁、国美
  - 支持图片类型：主图、分类图、详情图
  - 支持即时预览 显示 URL
  - 支持下载单张图片 右键另存为
  - 支持下载所有图片 打包为压缩文件并保存
- 淘宝店铺搜索卖家ID名采集
  - 支持忽略天猫店铺
- 天猫供销平台分销商一键邀请
  - 支持卖家账号登录 得到 Cookie（也可以手动输入 Cookie 字符串）
- 天猫供销平台分销商一键撤回
- 将来会有更多功能，随缘更新

## Development

#### Get the sources


In order to make development easier, the frontend is included in the backend as a [git submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules).

Then, in order to tinker with the sources, start by getting both repos at once with:

``` bash
git clone --recurse-submodules https://github.com/qwqcode/Nacollector.git
```

> NOTE: since it is a submodule, when developing the frontend remember to update the backend repo accordingly.

#### Quick Start

```bash
# 1. clone
git clone --recurse-submodules https://github.com/qwqcode/Nacollector.git
cd Nacollector

# 2. copy config files
cp ./Nacollector/GlobalConstant.cs.example ./Nacollector/GlobalConstant.cs

# 3. download `https://github.com/qwqcode/Nacollector/releases/download/1.3.0.0/CefSharp_v69.7z` to `./CefSharp/` and unpack

# 4. open .sln by vs
start Nacollector.sln
```

## Donate
如果您觉得我的项目对您有帮助，并且您愿意给予我一点小小的支持，您可以通过以下方式向我捐赠，这样可以维持项目持续地发展，非常感谢！ヽ(•̀ω•́ )ゝ

If you are enjoying this app, please consider making a donation to keep it alive.

| Alipay | Wechat | 
| :------: | :------: | 
| <img width="150" src="./docs/donate/alipay.png"> | <img width="150" src="./docs/donate/wechat.png"> | 

捐赠者的名字将保存于 [捐赠者列表](https://github.com/qwqcode/donate-qwqaq)，非常感谢你们的支持

## License

请务必仔细阅读 [《Nacollector 用户使用许可协议》](./LICENSE)

[Nacollector](https://github.com/qwqcode/Nacollector) Copyright (C) 2018 [qwqaq.com](https://qwqaq.com)

禁止擅自以任何收费形式盈利，禁止擅自修改版权信息，侵权必究！

## Screenshots
<p align="center">
<img src="./docs/screenshots/home.png">
<img src="./docs/screenshots/terminal.png">
<img src="./docs/screenshots/terminal1.png">
<img src="./docs/screenshots/terminal2.png">
<img src="./docs/screenshots/terminal3.png">
<img src="./docs/screenshots/terminal4.png">
<img src="./docs/screenshots/terminal5.png">
<img src="./docs/screenshots/terminal6.png">
<img src="./docs/screenshots/action.gif">
<img src="./docs/screenshots/cookie_getter.png">
<img src="./docs/screenshots/tasks.png">
<img src="./docs/screenshots/downloading.png">
<img src="./docs/screenshots/downloading1.png">
<img src="./docs/screenshots/settings.png">
<img src="./docs/screenshots/panel.gif">
</p>
