# LiveAutomation — 多平台直播自动化 RPA 工具

> 一个 Windows 桌面工具，通过 UI 自动化（RPA）驱动主流直播客户端，按计划自动**开播、下播、切换回放**，并联动 OBS。无需人工守着电脑，把直播间的开关流程交给程序。

![platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-9.0--windows-512BD4)
![UI](https://img.shields.io/badge/UI-WPF-2C5BB4)
![automation](https://img.shields.io/badge/RPA-FlaUI%20UIA3-orange)

---

## 解决什么问题

直播的「开播 / 下播 / 上回放」往往要卡着固定时间手动点客户端，多个平台、多个时段尤其折腾人。

LiveAutomation 把这些重复操作做成**定时任务**：到点自动唤起对应的直播客户端、点开播、按时下播、需要时切到录播回放，并能同步控制 OBS。一次配置，按表执行。

支持的平台：

- 🎵 **抖音**（抖音直播伴侣）
- 📺 **哔哩哔哩**（B 站直播）
- 💬 **微信视频号**
- 🎬 **OBS Studio**（推流 / 场景联动）

---

## 工作原理

程序本身不调用任何平台的私有接口，而是**模拟人在桌面上的操作**：

```
┌────────────────────────────────────────────────┐
│            MainWindow（任务调度中心）            │
│   DispatcherTimer 每秒轮询任务表，判断状态：     │
│   待准备 → 开播 → 下播 → 回放                    │
└───────────────┬────────────────────────────────┘
                │ 调用各平台 Controller
   ┌────────────┼─────────────┬──────────────┐
   ▼            ▼             ▼              ▼
抖音 Controller  B站 Controller  视频号 Controller  OBS Controller
   └──────── 基于 FlaUI(UIA3) + WinAPI 定位窗口/控件并点击 ────────┘
                │
                ▼
        SQLite（任务、配置、视频去重记录持久化）
```

- **UI 自动化**：基于 [FlaUI](https://github.com/FlaUI/FlaUI)(UIA3) 与 Win32 API（`FindWindow` / `SetForegroundWindow` / `ShowWindow`）定位并操作客户端窗口与控件。
- **任务调度**：`DispatcherTimer` 每秒轮询任务表，按时间推进每个任务的生命周期（准备 → 开播 → 下播 → 回放）。
- **数据持久化**：所有直播任务、平台配置存于本地 SQLite。
- **视频去重**：`VideoMd5Helper` 通过 MD5 识别回放视频，避免重复播放。
- **音频监测**：基于 NAudio（CoreAudioApi）监测音频设备状态。

---

## 功能特性

- ✅ 多平台统一管理：抖音 / B 站 / 微信视频号 / OBS 集中调度
- ✅ 定时开播与下播：按计划自动执行，无需人工值守
- ✅ 录播回放：到点自动切换到回放视频，支持回放参数设置
- ✅ 任务管理：新增 / 编辑 / 删除直播任务（`AddTaskWindow` / `EditTaskWindow`）
- ✅ 后台数据抓取：独立线程采集直播相关数据
- ✅ 视频去重：MD5 校验，避免回放重复内容

---

## 技术栈

| 类别 | 技术 |
|---|---|
| 运行平台 | Windows（`net9.0-windows`） |
| UI 框架 | WPF |
| UI 自动化 | FlaUI.Core / FlaUI.UIA3、OpenQA.Selenium.Winium |
| 数据库 | SQLite（Microsoft.Data.Sqlite、System.Data.SQLite、linq2db、EF Core） |
| 音频 | NAudio |
| 系统交互 | Win32 API（user32.dll） |

---

## 环境要求

- Windows 10 / 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022（17.14+）或 `dotnet` CLI
- 已安装对应平台的直播客户端（抖音直播伴侣 / B 站直播 / 微信 / OBS）

---

## 构建与运行

```bash
# 克隆仓库
git clone https://github.com/SHK0918/LiveAutomation.git
cd LiveAutomation

# 还原依赖并构建
dotnet restore
dotnet build -c Release

# 运行
dotnet run --project WpfAppLiveAutomation
```

或直接用 Visual Studio 打开 `WpfAppLiveAutomation.sln`，按 F5 运行。

> ⚠️ 首次使用前，需在程序里配置各直播客户端的安装路径与窗口标题，UI 自动化才能正确定位窗口。

---

## 目录结构

```
WpfAppLiveAutomation/
├── MainWindow.xaml(.cs)         # 主窗口：任务列表与调度中心
├── AddTaskWindow.xaml(.cs)      # 新增直播任务
├── EditTaskWindow.xaml(.cs)     # 编辑直播任务
├── ReplaySettingWindow.xaml(.cs)# 回放 / 录播设置
├── Controllers/
│   ├── AutoControllerBase.cs        # 自动化控制基类（窗口启动/激活）
│   ├── CoordinateControllerBase.cs  # 坐标点击基类
│   ├── DouyinLiveController.cs       # 抖音直播控制
│   ├── BilibiliLiveController.cs     # B 站直播控制
│   ├── WeChatliveController.cs       # 微信视频号控制
│   └── OBSController.cs              # OBS 控制
├── SQLiteHelper.cs              # SQLite 数据访问
└── VideoMd5Helper.cs           # 回放视频 MD5 去重
```

---

## 免责声明

本工具仅用于自动化个人合法的直播运营操作，请遵守各直播平台的用户协议与相关法律法规。因使用本工具产生的任何后果由使用者自行承担。
