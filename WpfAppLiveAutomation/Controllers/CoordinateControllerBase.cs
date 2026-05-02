using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static System.Net.Mime.MediaTypeNames;

[assembly: SupportedOSPlatform("windows")]
public abstract class LiveControllerBase
{
    // WinAPI 函数定义
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    protected static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    protected static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

    [DllImport("user32.dll")]
    protected static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    protected static extern bool SetForegroundWindow(IntPtr hWnd);

    protected const int SW_RESTORE = 9;
    protected const int SW_MAXIMIZE = 3;
    protected const int SW_SHOWMAXIMIZED = 3;

    protected FlaUI.Core.Application _application;
    protected UIA3Automation _automation;
    protected FlaUI.Core.AutomationElements.Window _window;
    protected IntPtr _windowHandle;

    // 抽象属性，子类必须实现
    protected abstract string ApplicationPath { get; }
    protected abstract string WindowTitle { get; }
    protected abstract int StartupDelay { get; }

    public void OpenAndActivate()
    {
        int processId = 0;
        _windowHandle = FindWindow(null, WindowTitle);

        if (_windowHandle == IntPtr.Zero)
        {
            Console.WriteLine($"未找到{WindowTitle}窗口，尝试启动...");
            try
            {
                Process.Start(ApplicationPath);
                Console.WriteLine($"已启动{WindowTitle}，等待{StartupDelay / 1000}秒让程序完全加载...");
                System.Threading.Thread.Sleep(StartupDelay);

                _windowHandle = FindWindow(null, WindowTitle);
                if (_windowHandle == IntPtr.Zero)
                {
                    Console.WriteLine($"启动后仍未找到窗口，请检查{WindowTitle}是否正常运行");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动{WindowTitle}失败: {ex.Message}");
                Console.WriteLine($"请检查是否安装在默认路径: {ApplicationPath}");
                return;
            }
        }

        GetWindowThreadProcessId(_windowHandle, out processId);
        Console.WriteLine($"找到{WindowTitle}窗口，进程ID: {processId}");

        try
        {
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures", true);
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures.2", true);
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures.3", true);

            _automation = new UIA3Automation();
            _application = FlaUI.Core.Application.Attach(processId);
            _window = _application.GetMainWindow(_automation);

            MaximizeWindow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"自动化操作失败: {ex.Message}");
            Console.WriteLine("尝试使用基础WinAPI激活...");
            FallbackActivate();
        }
    }

    protected virtual void MaximizeWindow()
    {
        try
        {
            if (_window != null)
            {
                _window.Patterns.Window.Pattern.SetWindowVisualState(
                    FlaUI.Core.Definitions.WindowVisualState.Maximized);
                _window.Focus();
                Console.WriteLine("通过FlaUI最大化窗口成功");
                return;
            }
        }
        catch
        {
            // 如果FlaUI失败，回退到WinAPI
        }

        ShowWindow(_windowHandle, SW_SHOWMAXIMIZED);
        SetForegroundWindow(_windowHandle);
        Console.WriteLine("通过WinAPI最大化窗口成功");
    }

    protected virtual void FallbackActivate()
    {
        ShowWindow(_windowHandle, SW_RESTORE);
        SetForegroundWindow(_windowHandle);
        Console.WriteLine("通过WinAPI激活成功");
    }
    public abstract void ClickStartLiveButton();

    public virtual void ExecuteStart(Task task)
    {
        Console.WriteLine($"开始执行任务: {task.TaskName}");
        Console.WriteLine($"平台: {task.Platform}");
        Console.WriteLine($"视频地址: {task.VideoAddress}");
        Console.WriteLine($"海报地址: {task.PosterAddress}");
    }

    public virtual void ExecuteEnd()
    {
        Console.WriteLine("结束直播操作");
    }

    [DllImport("user32.dll")]
    protected static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    protected static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    protected const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    protected const uint MOUSEEVENTF_LEFTUP = 0x0004;

    protected void ClickAtPosition(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
        System.Threading.Thread.Sleep(100);
        mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
    }
}