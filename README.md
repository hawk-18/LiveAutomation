# WpfAppLiveAutomation

直播自动化RPA工具，支持多平台直播控制。

## 功能特性

- **多平台支持**
  - Bilibili 直播
  - Douyin 直播
  - WeChat 视频号直播

- **自动化控制**
  - OBS 场景切换
  - 自动回复弹幕
  - 定时任务执行

- **任务管理**
  - 创建/编辑/删除任务
  - 任务计划调度
  -  replay设置

## 技术栈

- C# / .NET WPF
- SQLite 数据库

## 项目结构

```
WpfAppLiveAutomation/
├── Controllers/          # 直播平台控制器
│   ├── BilibiliLiveController.cs
│   ├── DouyinLiveController.cs
│   ├── WeChatliveController.cs
│   └── OBSController.cs
├── MainWindow.xaml        # 主窗口
├── AddTaskWindow.xaml    # 添加任务窗口
├── EditTaskWindow.xaml  # 编辑任务窗口
├── SQLiteHelper.cs      # 数据库辅助类
└── VideoMd5Helper.cs   # 视频MD5辅助类
```

## 使用说明

1. 克隆项目
```bash
git clone https://github.com/SHK0918/LiveAutomation.git
```

2. 使用 Visual Studio 打开 `WpfAppLiveAutomation.sln`

3. 编译运行

## 相关文档

- 海康威视视频监控系统集成
- WPF应用直播自动化

## License

MIT