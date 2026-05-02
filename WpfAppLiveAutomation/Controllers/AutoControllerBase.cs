using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
[assembly: SupportedOSPlatform("windows")]

public abstract class OBSControllerBase
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

    protected Application _application;
    protected UIA3Automation _automation;
    protected FlaUI.Core.AutomationElements.Window _window;
    // 添加这个方法用于关闭进程
    [DllImport("user32.dll")]
    private static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

    private const uint WM_CLOSE = 0x10;
    // 抽象属性，子类必须实现
    protected abstract string ApplicationPath { get; }
    protected abstract string WindowTitle { get; }

    public bool OpenApplication()
    {
        if (IsApplicationRunning())
        {
            return false;
        }

        try
        {
            string originalCwd = Directory.GetCurrentDirectory();
            string appDir = Path.GetDirectoryName(ApplicationPath);
            Directory.SetCurrentDirectory(appDir);

            Process.Start(ApplicationPath);

            Directory.SetCurrentDirectory(originalCwd);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动{WindowTitle}失败: {ex.Message}");
            return false;
        }
    }

    protected bool IsApplicationRunning()
    {
        IntPtr hwnd = FindWindow(null, WindowTitle);
        return hwnd != IntPtr.Zero;
    }

    public void OpenAndActivate()
    {
        if (!IsApplicationRunning())
        {
            Console.WriteLine($"{WindowTitle}未运行，尝试启动...");
            if (OpenApplication())
            {
                Console.WriteLine($"已启动{WindowTitle}，等待3秒让程序完全加载...");
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                Console.WriteLine($"启动{WindowTitle}失败");
                return;
            }
        }

        IntPtr hwnd = FindWindow(null, WindowTitle);
        if (hwnd == IntPtr.Zero)
        {
            Console.WriteLine($"启动{WindowTitle}后仍未找到窗口，请检查是否正常运行");
            return;
        }

        GetWindowThreadProcessId(hwnd, out int processId);
        Console.WriteLine($"找到{WindowTitle}窗口，进程ID: {processId}");

        try
        {
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures", true);
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures.2", true);
            AppContext.SetSwitch("System.Windows.Forms.UseLegacyAccessibilityFeatures.3", true);

            _automation = new UIA3Automation();
            _application = Application.Attach(processId);
            _window = _application.GetMainWindow(_automation);

            ActivateWindow(hwnd);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"自动化操作失败: {ex.Message}");
            Console.WriteLine("尝试使用基础WinAPI激活...");
            FallbackActivate(hwnd);
        }
    }

    protected virtual void ActivateWindow(IntPtr hwnd)
    {
        try
        {
            if (_window != null)
            {
                _window.Patterns.Window.Pattern.SetWindowVisualState(
                    FlaUI.Core.Definitions.WindowVisualState.Normal);
                _window.Focus();
                Console.WriteLine("通过FlaUI激活成功");
                return;
            }
        }
        catch
        {
            // 如果FlaUI失败，回退到WinAPI
        }
        FallbackActivate(hwnd);
    }

    protected virtual void FallbackActivate(IntPtr hwnd)
    {
        ShowWindow(hwnd, SW_RESTORE);
        SetForegroundWindow(hwnd);
        Console.WriteLine("通过WinAPI激活成功");
    }

    protected void ClickButtonById(string automationId, string buttonName)
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return;
            }

            var button = _window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (button != null)
            {
                button.AsButton().Click();
                Console.WriteLine($"成功点击按钮: {buttonName}");
            }
            else
            {
                Console.WriteLine($"未找到指定按钮: {buttonName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击按钮{buttonName}时出错: {ex.Message}");
        }
    }
    // 在 OBSControllerBase 类中添加以下成员
    [StructLayout(LayoutKind.Sequential)]
    protected struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    protected static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    protected void ClickAtPosition(int x, int y)
    {
        try
        {
            // 使用FlaUI的静态Mouse类
            Mouse.MoveTo(new System.Drawing.Point(x, y));
            Mouse.Click(MouseButton.Left);
            Console.WriteLine($"成功点击坐标: ({x}, {y})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击坐标({x},{y})失败: {ex.Message}");
        }
    }

    protected void ClearAndInputText(string text)
    {
        try
        {
            // 使用FlaUI的静态Keyboard类
            // 全选现有文本
            Keyboard.Press(VirtualKeyShort.CONTROL);
            Keyboard.Press(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.KEY_A);
            Keyboard.Release(VirtualKeyShort.CONTROL);
            Thread.Sleep(100);

            // 删除并输入新文本
            Keyboard.Press(VirtualKeyShort.DELETE);
            Keyboard.Release(VirtualKeyShort.DELETE);
            Thread.Sleep(100);

            Keyboard.Type(text);
            Console.WriteLine($"成功输入文本: {text}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"文本输入失败: {ex.Message}");
        }
    }


}
