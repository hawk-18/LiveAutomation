using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WeChatLiveController : LiveControllerBase
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
    private const int INPUTLANGCHANGEREQUEST_SYSCHARSET = 0x0001;
    private const int KLF_ACTIVATE = 0x00000001;

    protected override string ApplicationPath => @"D:\wechat\Weixin\Weixin.exe";
    protected override string WindowTitle => "微信";
    protected override int StartupDelay => 30000;

    private void SetEnglishKeyboardLayout()
    {
        try
        {
            LoadKeyboardLayout("00000409", KLF_ACTIVATE);
            IntPtr hwnd = GetForegroundWindow();
            PostMessage(hwnd, WM_INPUTLANGCHANGEREQUEST,
                       INPUTLANGCHANGEREQUEST_SYSCHARSET,
                       0x04090409);
            Thread.Sleep(200);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"切换输入法失败: {ex.Message}");
        }
    }

    public bool ClickWeChatButtons()
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }

            var windowRect = _window.BoundingRectangle;

            int button1X = (int)(windowRect.Width * 0.01);
            int button1Y = (int)(windowRect.Height * 0.95);
            int absoluteX1 = (int)windowRect.X + button1X;
            int absoluteY1 = (int)windowRect.Y + button1Y;
            ClickAtPosition(absoluteX1, absoluteY1);
            Console.WriteLine($"成功点击微信设置按钮");
            Thread.Sleep(1000);

            int button2X = (int)(windowRect.Width * 0.07);
            int button2Y = (int)(windowRect.Height * 0.78);
            int absoluteX2 = (int)windowRect.X + button2X;
            int absoluteY2 = (int)windowRect.Y + button2Y;
            ClickAtPosition(absoluteX2, absoluteY2);
            Console.WriteLine($"成功点击视频直播工具按钮");
            Thread.Sleep(5000);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击按钮时出错: {ex.Message}");
            return false;
        }
    }

    public bool ActivateLiveToolWindow()
    {
        try
        {
            Console.WriteLine("尝试激活视频号直播工具窗口...");
            IntPtr liveToolHwnd = FindWindow(null, "视频号直播伴侣");
            if (liveToolHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到视频号直播工具窗口，等待3秒后重试...");
                Thread.Sleep(3000);
                liveToolHwnd = FindWindow(null, "视频号直播伴侣");

                if (liveToolHwnd == IntPtr.Zero)
                {
                    Console.WriteLine("仍然未找到视频号直播工具窗口，请检查是否已正确打开");
                    return false;
                }
            }

            ShowWindow(liveToolHwnd, SW_RESTORE);
            SetForegroundWindow(liveToolHwnd);
            Console.WriteLine("视频号直播工具窗口激活成功");
            Thread.Sleep(1000);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"激活视频号直播工具窗口时出错: {ex.Message}");
            return false;
        }
    }

    public override void ClickStartLiveButton()
    {
        ClickStartLiveButton();
    }

    public bool ClickStartLiveButton(string liveTopic, Task task)
    {
        try
        {
            DragInLiveToolWindow(0.26, 0.23, 0.26, 0.26);
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }

            var windowRect = _window.BoundingRectangle;
            if (windowRect == Rectangle.Empty)
            {
                Console.WriteLine("无法获取窗口位置");
                return false;
            }

            // 点击第一个按钮
            int buttonX = (int)(windowRect.Width * 0.60);
            int buttonY = (int)(windowRect.Height * 0.74);
            int absoluteX = (int)windowRect.X + buttonX;
            int absoluteY = (int)windowRect.Y + buttonY;
            ClickAtPosition(absoluteX, absoluteY);
            Thread.Sleep(1000);

            // 点击第二个按钮
            int buttonX1 = (int)(windowRect.Width * 0.46);
            int buttonY1 = (int)(windowRect.Height * 0.54);
            int absoluteX1 = (int)windowRect.X + buttonX1;
            int absoluteY1 = (int)windowRect.Y + buttonY1;
            ClickAtPosition(absoluteX1, absoluteY1);

            if (!ActivateLiveSettingsWindow())
            {
                Console.WriteLine("无法激活直播设置窗口");
                return false;
            }

            // 点击海报设置按钮
            double btn1X = 0.42;
            double btn1Y = 0.49;
            int absBtn1X = (int)(windowRect.X + windowRect.Width * btn1X);
            int absBtn1Y = (int)(windowRect.Y + windowRect.Height * btn1Y);
            ClickAtPosition(absBtn1X, absBtn1Y);
            Thread.Sleep(3000);

            // 输入海报地址
            ClearAndInputText(task.PosterAddress);
            Console.WriteLine($"已输入海报地址: {task.PosterAddress}");
            Thread.Sleep(500);

            // 点击确认按钮
            double btn3X = 0.76;
            double btn3Y = 0.78;
            int absBtn3X = (int)(windowRect.X + windowRect.Width * btn3X);
            int absBtn3Y = (int)(windowRect.Y + windowRect.Height * btn3Y);
            ClickAtPosition(absBtn3X, absBtn3Y);

            Thread.Sleep(2500);



            // 输入直播主题
            double textBoxRelativeX = 0.51;
            double textBoxRelativeY = 0.35;
            int textBoxX = (int)(windowRect.X + windowRect.Width * textBoxRelativeX);
            int textBoxY = (int)(windowRect.Y + windowRect.Height * textBoxRelativeY);

            Console.WriteLine($"准备点击文本框位置: X={textBoxX}, Y={textBoxY}");
            ClickAtPosition(textBoxX, textBoxY);
            Thread.Sleep(500);

            ClearAndInputText(liveTopic);
            Console.WriteLine($"已输入直播主题: {liveTopic}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击开始直播按钮时出错: {ex.Message}");
            return false;
        }
    }


    public bool ActivateLiveSettingsWindow()
    {
        try
        {
            Console.WriteLine("尝试激活开播设置窗口...");
            IntPtr liveSettingsHwnd = FindWindow(null, "开播设置");
            if (liveSettingsHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到开播设置窗口，等待2秒后重试...");
                Thread.Sleep(2000);
                liveSettingsHwnd = FindWindow(null, "开播设置");

                if (liveSettingsHwnd == IntPtr.Zero)
                {
                    Console.WriteLine("仍然未找到开播设置窗口，请检查是否已正确打开");
                    return false;
                }
            }

            ShowWindow(liveSettingsHwnd, SW_RESTORE);
            SetForegroundWindow(liveSettingsHwnd);
            Console.WriteLine("开播设置窗口激活成功");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"激活开播设置窗口时出错: {ex.Message}");
            return false;
        }
    }

    public bool FillLiveTopicByRelativePosition(string topicText)
    {
        try
        {
            if (string.IsNullOrEmpty(topicText) || topicText.Length < 5 || topicText.Length > 15)
            {
                Console.WriteLine("主题文本长度必须在5-15个字符之间");
                return false;
            }

            IntPtr liveSettingsHwnd = FindWindow(null, "开播设置");
            if (liveSettingsHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到开播设置窗口");
                return false;
            }

            var windowRect = GetWindowRect(liveSettingsHwnd);
            if (windowRect == Rectangle.Empty)
            {
                Console.WriteLine("无法获取窗口位置");
                return false;
            }

            //double textBoxRelativeX = 0.51;
            //double textBoxRelativeY = 0.15;
            //有预约消息如下坐标：
            double textBoxRelativeX = 0.51;
            double textBoxRelativeY = 0.13;
            int textBoxX = (int)(windowRect.Width * textBoxRelativeX);
            int textBoxY = (int)(windowRect.Height * textBoxRelativeY);
            int absoluteX = windowRect.X + textBoxX;
            int absoluteY = windowRect.Y + textBoxY;

            Console.WriteLine($"准备点击文本框位置: X={absoluteX}, Y={absoluteY}");
            ClickAtPosition(absoluteX, absoluteY);
            Thread.Sleep(500);

            ClearAndInputText(topicText);


            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"通过相对坐标填充文字时出错: {ex.Message}");
            return false;
        }
    }

    public bool StartLiveBroadcast()
    {
        try
        {
            if (!ActivateLiveSettingsWindow())
            {
                Console.WriteLine("无法激活开播设置窗口");
                return false;
            }

            IntPtr liveSettingsHwnd = FindWindow(null, "开播设置");
            if (liveSettingsHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到开播设置窗口");
                return false;
            }

            var windowRect = GetWindowRect(liveSettingsHwnd);
            if (windowRect == Rectangle.Empty)
            {
                Console.WriteLine("无法获取窗口位置");
                return false;
            }

            double finalButtonRelativeX = 0.43;
            double finalButtonRelativeY = 0.75;
            int finalButtonX = (int)(windowRect.Width * finalButtonRelativeX);
            int finalButtonY = (int)(windowRect.Height * finalButtonRelativeY);
            int finalButtonAbsoluteX = windowRect.X + finalButtonX;
            int finalButtonAbsoluteY = windowRect.Y + finalButtonY;

            Console.WriteLine($"准备点击开始直播按钮位置: X={finalButtonAbsoluteX}, Y={finalButtonAbsoluteY}");
            ClickAtPosition(finalButtonAbsoluteX, finalButtonAbsoluteY);
            Thread.Sleep(1000);
            ClickAtPosition(finalButtonAbsoluteX, finalButtonAbsoluteY);
            Console.WriteLine("直播已成功开始!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"开始直播时出错: {ex.Message}");
            return false;
        }
    }
   public bool StopLiveBroadcast()
{
    try
    {
        // 第一步：找到并点击"结束直播"按钮
        IntPtr liveWindowHwnd = FindWindow(null, "视频号直播伴侣");
        if (liveWindowHwnd == IntPtr.Zero)
        {
            Console.WriteLine("未找到直播窗口");
            return false;
        }

        var windowRect = GetWindowRect(liveWindowHwnd);
        if (windowRect == Rectangle.Empty)
        {
            Console.WriteLine("无法获取直播窗口位置");
            return false;
        }

        // 第一个按钮：结束直播按钮的相对位置（示例值，需要根据实际UI调整）
        double stopButtonRelativeX = 0.65;
        double stopButtonRelativeY = 0.94;
        int stopButtonX = (int)(windowRect.Width * stopButtonRelativeX);
        int stopButtonY = (int)(windowRect.Height * stopButtonRelativeY);
        int stopButtonAbsoluteX = windowRect.X + stopButtonX;
        int stopButtonAbsoluteY = windowRect.Y + stopButtonY;

        Console.WriteLine($"准备点击结束直播按钮位置: X={stopButtonAbsoluteX}, Y={stopButtonAbsoluteY}");
        ClickAtPosition(stopButtonAbsoluteX, stopButtonAbsoluteY);

            // 等待确认对话框出现
            Thread.Sleep(1000); // 等待1秒让确认对话框弹出

            windowRect = GetWindowRect(liveWindowHwnd);

            // 第二个按钮：确认按钮的相对位置
            double confirmButtonRelativeX = 0.45;
            double confirmButtonRelativeY = 0.54;
            int confirmButtonX = (int)(windowRect.Width * confirmButtonRelativeX);
            int confirmButtonY = (int)(windowRect.Height * confirmButtonRelativeY);
            int confirmButtonAbsoluteX = windowRect.X + confirmButtonX;
            int confirmButtonAbsoluteY = windowRect.Y + confirmButtonY;

            Console.WriteLine($"准备点击确认结束按钮位置: X={confirmButtonAbsoluteX}, Y={confirmButtonAbsoluteY}");
            ClickAtPosition(confirmButtonAbsoluteX, confirmButtonAbsoluteY);

            Console.WriteLine("直播已成功结束!");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"结束直播时出错: {ex.Message}");
        return false;
    }


}
    
   public void StartplaybacktaskBroadcast()
    {

        IntPtr liveWindowHwnd = FindWindow(null, "视频号直播伴侣");


        var windowRect = GetWindowRect(liveWindowHwnd);

        double confirmButtonRelativeX = 0.45;
        double confirmButtonRelativeY = 0.54;
        int confirmButtonX = (int)(windowRect.Width * confirmButtonRelativeX);
        int confirmButtonY = (int)(windowRect.Height * confirmButtonRelativeY);
        int confirmButtonAbsoluteX = windowRect.X + confirmButtonX;
        int confirmButtonAbsoluteY = windowRect.Y + confirmButtonY;

        Console.WriteLine($"准备点击确认结束按钮位置: X={confirmButtonAbsoluteX}, Y={confirmButtonAbsoluteY}");
        ClickAtPosition(confirmButtonAbsoluteX, confirmButtonAbsoluteY);


    }
    // 在类顶部添加必要的DllImport
    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    // 添加鼠标拖拽方法实现
    private void MouseDrag(int startX, int startY, int endX, int endY)
    {
        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;
        const int MOUSEEVENTF_MOVE = 0x0001;

        SetCursorPos(startX, startY);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        Thread.Sleep(100);

        // 平滑移动
        int steps = 10;
        for (int i = 1; i <= steps; i++)
        {
            int currentX = startX + (endX - startX) * i / steps;
            int currentY = startY + (endY - startY) * i / steps;
            SetCursorPos(currentX, currentY);
            Thread.Sleep(50);
        }

        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    // 添加相对坐标拖拽方法
    public void DragInLiveToolWindow(double startRelativeX, double startRelativeY,
                                     double endRelativeX, double endRelativeY)
    {
        try
        {
            IntPtr liveToolHwnd = FindWindow(null, "视频号直播伴侣");
            if (liveToolHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到视频号直播工具窗口");
                return;
            }

            var windowRect = GetWindowRect(liveToolHwnd);
            if (windowRect == Rectangle.Empty)
            {
                Console.WriteLine("无法获取窗口位置");
                return;
            }

            // 计算绝对坐标
            int startX = windowRect.X + (int)(windowRect.Width * startRelativeX);
            int startY = windowRect.Y + (int)(windowRect.Height * startRelativeY);
            int endX = windowRect.X + (int)(windowRect.Width * endRelativeX);
            int endY = windowRect.Y + (int)(windowRect.Height * endRelativeY);

            // 执行拖拽
            MouseDrag(startX, startY, endX, endY);
            Console.WriteLine($"完成从 ({startRelativeX}, {startRelativeY}) 到 ({endRelativeX}, {endRelativeY}) 的拖拽");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行拖拽操作时出错: {ex.Message}");
        }
    }

 

    private void ClearAndInputText(string text)
    {
        try
        {
            SetEnglishKeyboardLayout();
            SendKeys.SendWait("^{HOME}");
            Thread.Sleep(50);
            SendKeys.SendWait("^+{END}");
            Thread.Sleep(50);
            SendKeys.SendWait("{DEL}");
            Thread.Sleep(200);

            foreach (char c in text)
            {
                SendKeys.SendWait("{" + c.ToString() + "}");
                Thread.Sleep(150);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"输入失败: {ex.Message}");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private Rectangle GetWindowRect(IntPtr hwnd)
    {
        RECT rect;
        if (GetWindowRect(hwnd, out rect))
        {
            return new Rectangle(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top);
        }
        return Rectangle.Empty;
    }
}
