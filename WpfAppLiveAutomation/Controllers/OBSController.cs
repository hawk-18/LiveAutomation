using FlaUI.Core.Input;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
public class OBSController : OBSControllerBase
{   
    protected override string ApplicationPath => @"D:\OBS\obs-studio\bin\64bit\obs64.exe";
    protected override string WindowTitle => "OBS 31.0.3 - 配置文件: 未命名 - 场景: 未命名";
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;
    private const int INPUTLANGCHANGEREQUEST_SYSCHARSET = 0x0001;
    private const int KLF_ACTIVATE = 0x00000001;
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
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private void SendKeyToWindow(IntPtr hWnd, byte keyCode)
    {
        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x101;
        SendMessage(hWnd, WM_KEYDOWN, keyCode, 0);
        Thread.Sleep(50);
        SendMessage(hWnd, WM_KEYUP, keyCode, 0);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

    public void ClickButton1()
    {
        ClickButtonById("OBSBasic.centralwidget.previewContainer.preview", "屏幕激活");
        ClickButtonById("OBSBasic.centralwidget.contextContainer.emptySpace.MediaControls.playPauseButton", "暂停播放");
    }

    public void ClickButton2()
    {
        ClickButtonById("OBSBasic.controlsDock.OBSBasicControls.controlsFrame.virtualCamButton", "启动虚拟摄像头");
    }

    public void ClickButton3()
    {
        ClickButtonById("OBSBasic.centralwidget.previewContainer.preview", "屏幕激活");
        ClickButtonById("OBSBasic.centralwidget.contextContainer.emptySpace.MediaControls.playPauseButton", "开始直播");
    }
    public bool ClickprepareLiveButton(string liveTopic, string VideoAddress)
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法操作");
                return false;
            }

            // 获取主窗口位置
            var windowRect = _window.BoundingRectangle;

            // === 步骤1: 连续点击三个不同位置的相对坐标 ===
            // 点击位置1
            double point1X = 0.39;
            double point1Y = 0.49;
            int absX1 = (int)(windowRect.X + windowRect.Width * point1X);
            int absY1 = (int)(windowRect.Y + windowRect.Height * point1Y);
            ClickAtPosition(absX1, absY1);
            Thread.Sleep(800);

            // 点击位置2
            double point2X = 0.21;
            double point2Y = 0.95;
            int absX2 = (int)(windowRect.X + windowRect.Width * point2X);
            int absY2 = (int)(windowRect.Y + windowRect.Height * point2Y);
            ClickAtPosition(absX2, absY2);
            Thread.Sleep(800);

            // 点击位置3
            double point3X = 0.24;
            double point3Y = 0.60;
            int absX3 = (int)(windowRect.X + windowRect.Width * point3X);
            int absY3 = (int)(windowRect.Y + windowRect.Height * point3Y);
            ClickAtPosition(absX3, absY3);
            Thread.Sleep(1000);

            // === 步骤2: 在输入框中输入文本 ===
            // 设置英文输入法（避免中文输入法干扰）
            SetEnglishKeyboardLayout();
            Thread.Sleep(200);

            // 定位并点击文本输入框
            double textBoxX = 0.45;
            double textBoxY = 0.39;
            int absTextX = (int)(windowRect.X + windowRect.Width * textBoxX);
            int absTextY = (int)(windowRect.Y + windowRect.Height * textBoxY);
            Console.WriteLine($"点击文本框坐标: X={absTextX}, Y={absTextY}");  // 添加调试日志
            ClickAtPosition(absTextX, absTextY);
            Thread.Sleep(300);

            // 获取当前日期时间并格式化为"MMddHHmm"格式
            string currentTime = DateTime.Now.ToString("MMddHHmm");

            // 清空并输入直播主题，然后追加当前时间
            ClearAndInputText(liveTopic + " " + currentTime);
            Thread.Sleep(2000);


            // === 步骤3: 点击文件选择按钮 ===
            double fileButtonX = 0.53;
            double fileButtonY = 0.62;
            int absFileX = (int)(windowRect.X + windowRect.Width * fileButtonX);
            int absFileY = (int)(windowRect.Y + windowRect.Height * fileButtonY);
            ClickAtPosition(absFileX, absFileY);
            double fileButtonX1 = 0.65;
            double fileButtonY1 = 0.53;
            int absFileX1 = (int)(windowRect.X + windowRect.Width * fileButtonX1);
            int absFileY1 = (int)(windowRect.Y + windowRect.Height * fileButtonY1);
            ClickAtPosition(absFileX1, absFileY1);
            //double fileButtonX = 0.69;
            //double fileButtonY = 0.9;
            //int absFileX = (int)(windowRect.X + windowRect.Width * fileButtonX);
            //int absFileY = (int)(windowRect.Y + windowRect.Height * fileButtonY);
            //ClickAtPosition(absFileX, absFileY);
            //double fileButtonX1 = 0.9;
            //double fileButtonY1 = 0.56;
            //int absFileX1 = (int)(windowRect.X + windowRect.Width * fileButtonX1);
            //int absFileY1 = (int)(windowRect.Y + windowRect.Height * fileButtonY1);

            // === 步骤4: 直接输入完整路径并确认 ===
            // 1. 点击文件选择按钮（相对坐标0.65,0.53）

            // 2. 聚焦到文件名输入框（保持原坐标0.53,0.67）
            //int nameBoxX = dialogRect.Left + (int)((dialogRect.Right - dialogRect.Left) * 0.53);
            //int nameBoxY = dialogRect.Top + (int)((dialogRect.Bottom - dialogRect.Top) * 0.67);
            //ClickAtPosition(nameBoxX, nameBoxY);
            Thread.Sleep(300);
            double fileButtonX2 = 0.53;
            double fileButtonY2 = 0.67;
            int absFileX2 = (int)(windowRect.X + windowRect.Width * fileButtonX2);
            int absFileY2 = (int)(windowRect.Y + windowRect.Height * fileButtonY2);
            ClickAtPosition(absFileX2, absFileY2);
            // 输入完整文件路径（自动添加.mp4后缀）
            string finalPath = VideoAddress;
            // 确保文件名以.mp4结尾
            if (!finalPath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                // 移除现有扩展名（如果存在）
                int lastDot = finalPath.LastIndexOf('.');
                int lastSlash = Math.Max(finalPath.LastIndexOf('\\'), finalPath.LastIndexOf('/'));

                if (lastDot > lastSlash && lastDot > 0)
                {
                    finalPath = finalPath.Substring(0, lastDot);
                }
                finalPath += ".mp4";
            }
            ClearAndInputText(finalPath);
            Thread.Sleep(500);
            // 3. 点击打开按钮（保持原坐标0.72,0.71）
            Thread.Sleep(300);
            double fileButtonX3 = 0.72;
            double fileButtonY3 = 0.71;
            int absFileX3 = (int)(windowRect.X + windowRect.Width * fileButtonX3);
            int absFileY3 = (int)(windowRect.Y + windowRect.Height * fileButtonY3);
            ClickAtPosition(absFileX3, absFileY3);
            Thread.Sleep(2000);
            double fileButtonX4 = 0.63;
            double fileButtonY4 = 0.75;
            int absFileX4 = (int)(windowRect.X + windowRect.Width * fileButtonX4);
            int absFileY4 = (int)(windowRect.Y + windowRect.Height * fileButtonY4);
            ClickAtPosition(absFileX4, absFileY4);

            Thread.Sleep(1000);
            double[,] clickPoints = {
            {0.42, 0.95},  // 位置2
            {0.68, 0.39},  // 位置3
            {0.68, 0.44},  // 位置4
            {0.76, 0.62}   // 位置5
        };

            for (int i = 0; i < 4; i++)
            {
                double relX = clickPoints[i, 0];
                double relY = clickPoints[i, 1];
                int absX = (int)(windowRect.X + windowRect.Width * relX);
                int absY = (int)(windowRect.Y + windowRect.Height * relY);
                ClickAtPosition(absX, absY);
                Thread.Sleep(i == 4 ? 1000 : 600);  // 最后一步等待稍长
            }
            
            



            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"准备直播时出错: {ex.Message}");
            return false;
        }
    }
    //public bool ClickprepareLiveButton1()
    //{
    //    // 获取主窗口位置
    //    var windowRect = _window.BoundingRectangle;
    //    // === 步骤5: 点击五个不同位置的相对坐标 ===
    //    // 坐标序列
    //    double[,] clickPoints = {
    //        {0.42, 0.95},  // 位置2
    //        {0.68, 0.39},  // 位置3
    //        {0.68, 0.44},  // 位置4
    //        {0.76, 0.62}   // 位置5
    //    };

    //    for (int i = 0; i < 4; i++)
    //    {
    //        double relX = clickPoints[i, 0];
    //        double relY = clickPoints[i, 1];
    //        int absX = (int)(windowRect.X + windowRect.Width * relX);
    //        int absY = (int)(windowRect.Y + windowRect.Height * relY);
    //        ClickAtPosition(absX, absY);
    //        Thread.Sleep(i == 4 ? 1000 : 600);  // 最后一步等待稍长
    //    }


    //return true;
    //}

}
