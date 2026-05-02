using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
public class DouyinLiveController : LiveControllerBase
{
    protected override string ApplicationPath => @"D:\douyin\webcast_mate\直播伴侣 Launcher.exe";
    protected override string WindowTitle => "直播伴侣";
    protected override int StartupDelay => 30000;
  
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
    public override void ClickStartLiveButton()
    {
        const int maxRetryCount = 5;
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return;
            }

            var windowRect = _window.BoundingRectangle;
            int buttonX = (int)(windowRect.Width * 0.8);
            int buttonY = (int)(windowRect.Height * 0.93);
            int absoluteX = (int)windowRect.X + buttonX;
            int absoluteY = (int)windowRect.Y + buttonY;
            ClickAtPosition(absoluteX, absoluteY);
        }
        catch (Exception ex) { 
        }
        //    // 失败时的两个重试按钮坐标（根据实际UI调整）
        //    int retryButton1X = (int)(windowRect.X + windowRect.Width * 0.02);
        //    int retryButton1Y = (int)(windowRect.Y + windowRect.Height * 0.09);
        //    int retryButton2X = (int)(windowRect.X + windowRect.Width * 0.8);
        //    int retryButton2Y = (int)(windowRect.Y + windowRect.Height * 0.93);

        //    // 颜色检测方法
        //    bool IsBlackTextPresent(int x, int y)
        //    {
        //        try
        //        {
        //            using (Bitmap screen = new Bitmap(1, 1))
        //            using (Graphics g = Graphics.FromImage(screen))
        //            {
        //                g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
        //                Color pixel = screen.GetPixel(0, 0);
        //                // 白色检测（RGB值接近255,255,255）
        //                return pixel.R < 30 && pixel.< 30  && pixel.B < 30 ;
        //            }
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }

        //    // 主操作流程
        //    for (int attempt = 1; attempt <= maxRetryCount; attempt++)
        //    {
        //        // 点击开始直播按钮
        //        ClickAtPosition(absoluteX, absoluteY);
        //        Console.WriteLine($"尝试 #{attempt}: 已点击开始直播按钮");
        //        Thread.Sleep(1500); // 等待界面响应

        //        // 检测黑体文字
        //        if (IsBlackTextPresent(958, 518))
        //        {
        //            return;
        //        }

        //        // 失败时点击重试按钮
        //        ClickAtPosition(retryButton1X, retryButton1Y);
        //        Thread.Sleep(500);
        //        ClickAtPosition(retryButton2X, retryButton2Y);
        //        Thread.Sleep(1000); // 等待重试操作完成
        //    }

        //    Console.WriteLine($"操作失败：达到最大重试次数 {maxRetryCount}");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"点击按钮时出错: {ex.Message}");
        //}
    }

    //public override void ClickStartLiveButton()
    //{
    //    try
    //    {
    //        if (_window == null)
    //        {
    //            Console.WriteLine("窗口未初始化，无法点击按钮");
    //            return;
    //        }

    //        var windowRect = _window.BoundingRectangle;
    //        int buttonX = (int)(windowRect.Width * 0.8);
    //        int buttonY = (int)(windowRect.Height * 0.93);
    //        //int buttonX = (int)(windowRect.Width * 0.5);
    //        //int buttonY = (int)(windowRect.Height * 0.5);
    //        int absoluteX = (int)windowRect.X + buttonX;
    //        int absoluteY = (int)windowRect.Y + buttonY;

    //        ClickAtPosition(absoluteX, absoluteY);
    //        Console.WriteLine($"成功点击开始直播按钮");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"点击按钮时出错: {ex.Message}");
    //    }
    //}


    public bool ActivateLiveSettingsWindow()
    {
        try
        {
            Console.WriteLine("尝试激活抖音直播设置窗口...");
            // 抖音直播设置窗口标题可能是"开播设置"或"直播设置"
            IntPtr liveSettingsHwnd = FindWindow(null, "开播设置");
            if (liveSettingsHwnd == IntPtr.Zero)
            {
                liveSettingsHwnd = FindWindow(null, "直播设置");
            }

            if (liveSettingsHwnd == IntPtr.Zero)
            {
                Console.WriteLine("未找到直播设置窗口，等待2秒后重试...");
                Thread.Sleep(2000);
                liveSettingsHwnd = FindWindow(null, "开播设置");
                if (liveSettingsHwnd == IntPtr.Zero)
                {
                    liveSettingsHwnd = FindWindow(null, "直播设置");
                }

                if (liveSettingsHwnd == IntPtr.Zero)
                {
                    Console.WriteLine("仍然未找到直播设置窗口");
                    return false;
                }
            }

            ShowWindow(liveSettingsHwnd, SW_RESTORE);
            SetForegroundWindow(liveSettingsHwnd);
            Console.WriteLine("抖音直播设置窗口激活成功");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"激活窗口出错: {ex.Message}");
            return false;
        }
    }
    //public bool ClickPrepareLiveButton(string liveTopic)
    //{
    //    try
    //    {
    //        if (_window == null)
    //        {
    //            Console.WriteLine("窗口未初始化，无法点击按钮");
    //            return false;
    //        }

    //        var windowRect = _window.BoundingRectangle;
    //        int buttonX = (int)(windowRect.Width * 0.28);
    //        int buttonY = (int)(windowRect.Height * 0.04);
    //        int absoluteX = (int)windowRect.X + buttonX;
    //        int absoluteY = (int)windowRect.Y + buttonY;

    //        ClickAtPosition(absoluteX, absoluteY);
    //        Thread.Sleep(2000);  // 等待设置窗口弹出


    //        // 激活直播设置窗口并输入标题
    //        return FillLiveTopic(liveTopic);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"开播流程出错: {ex.Message}");
    //        return false;
    //    }
    //}
    //public bool FillLiveTopic(string topicText)
    //{
    //    try
    //    {
    //        // 抖音标题长度验证（根据实际要求调整）
    //        if (string.IsNullOrEmpty(topicText) || topicText.Length < 2 || topicText.Length > 40)
    //        {
    //            Console.WriteLine("标题长度需在2-40字符之间");
    //            return false;
    //        }

    //        if (!ActivateLiveSettingsWindow()) return false;

    //        IntPtr settingsHwnd = FindWindow(null, "开播设置");
    //        if (settingsHwnd == IntPtr.Zero)
    //        {
    //            settingsHwnd = FindWindow(null, "直播设置");
    //        }

    //        if (settingsHwnd == IntPtr.Zero)
    //        {
    //            Console.WriteLine("未找到直播设置窗口");
    //            return false;
    //        }

    //        var windowRect = GetWindowRect(settingsHwnd);
    //        if (windowRect == Rectangle.Empty) return false;

    //        // 抖音标题输入框相对位置（根据实际UI调整）
    //        double titleBoxX = 0.5;
    //        double titleBoxY = 0.3;
    //        int absX = windowRect.X + (int)(windowRect.Width * titleBoxX);
    //        int absY = windowRect.Y + (int)(windowRect.Height * titleBoxY);

    //        Console.WriteLine($"点击标题输入框位置: ({absX}, {absY})");
    //        ClickAtPosition(absX, absY);
    //        Thread.Sleep(500);

    //        // 清空并输入新标题
    //        ClearAndInputText(topicText);
    //        Console.WriteLine($"已输入标题: {topicText}");
    //        Thread.Sleep(500);

    //        // 点击确定按钮（在设置窗口中）
    //        double startButtonX = 0.5;
    //        double startButtonY = 0.75;
    //        int startAbsX = windowRect.X + (int)(windowRect.Width * startButtonX);
    //        int startAbsY = windowRect.Y + (int)(windowRect.Height * startButtonY);

    //        Console.WriteLine($"点击确定按钮位置: ({startAbsX}, {startAbsY})");
    //        ClickAtPosition(startAbsX, startAbsY);
    //        Thread.Sleep(2000);

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"填写标题出错: {ex.Message}");
    //        return false;
    //    }
    //}
    public bool ClickPrepareLiveButton(string liveTopic,Task task)
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }

            // 标题长度验证
            if (string.IsNullOrEmpty(liveTopic) || liveTopic.Length < 2 || liveTopic.Length > 40)
            {
                Console.WriteLine("标题长度需在2-40字符之间");
                return false;
            }

            var windowRect = _window.BoundingRectangle;

            // === 主按钮点击 ===
            double mainBtnX = 0.23;
            double mainBtnY = 0.065;
            int absMainX = (int)(windowRect.X + windowRect.Width * mainBtnX);
            int absMainY = (int)(windowRect.Y + windowRect.Height * mainBtnY);
            ClickAtPosition(absMainX, absMainY);
            Thread.Sleep(500);  // 等待设置窗口弹出

            // === 标题输入流程 ===
            // 设置英文输入法
            SetEnglishKeyboardLayout();
            Thread.Sleep(200);

            //// 定位标题输入框
            //double titleBoxX = 0.5;
            //double titleBoxY = 0.3;
            //int absTitleX = windowRect.X + (int)(windowRect.Width * titleBoxX);
            //int absTitleY = windowRect.Y + (int)(windowRect.Height * titleBoxY);

            //Console.WriteLine($"点击标题输入框位置: ({absTitleX}, {absTitleY})");
            //ClickAtPosition(absTitleX, absTitleY);
            //Thread.Sleep(500);  // 等待文本框激活

            // 清空并输入标题
            ClearAndInputText(liveTopic);
            Console.WriteLine($"已输入标题: {liveTopic}");
            Thread.Sleep(800);  // 确保输入完成

            //// === 确定按钮点击 ===
            //double confirmX = 0.5;
            //double confirmY = 0.75;
            //int absConfirmX = windowRect.X + (int)(windowRect.Width * confirmX);
            //int absConfirmY = windowRect.X + (int)(windowRect.Width * confirmY);

            //Console.WriteLine($"点击确定按钮位置: ({absConfirmX}, {absConfirmY})");
            //ClickAtPosition(absConfirmX, absConfirmY);
            //Thread.Sleep(2000);  // 等待操作生效

            // === 1. 点击两个不同的按钮 ===
            // 第一个按钮点击
            double btn1X = 0.31;
            double btn1Y = 0.065;
            int absBtn1X = (int)(windowRect.X + windowRect.Width * btn1X);
            int absBtn1Y = (int)(windowRect.Y + windowRect.Height * btn1Y);
            ClickAtPosition(absBtn1X, absBtn1Y);
            Console.WriteLine("已点击第一个按钮");
            Thread.Sleep(10000);

            // 第二个按钮点击
            double btn2X = 0.39;
            double btn2Y = 0.32;
            int absBtn2X = (int)(windowRect.X + windowRect.Width * btn2X);
            int absBtn2Y = (int)(windowRect.Y + windowRect.Height * btn2Y);
            ClickAtPosition(absBtn2X, absBtn2Y);
            Console.WriteLine("已点击第二个按钮");
            Thread.Sleep(2000);

            // === 2. 在文件选择器中输入 tasks.PosterAddress 并点击确认 ===
            if (!string.IsNullOrEmpty(task.PosterAddress))
            {
                // 点击文件选择按钮
                double fileSelectX = 0.21;
                double fileSelectY = 0.46;
                int absFileSelectX = (int)(windowRect.X + windowRect.Width * fileSelectX);
                int absFileSelectY = (int)(windowRect.Y + windowRect.Height * fileSelectY);
                ClickAtPosition(absFileSelectX, absFileSelectY);
                Thread.Sleep(1000);

                // 输入海报地址
                ClearAndInputText(task.PosterAddress);
                Console.WriteLine($"已输入海报地址: {task.PosterAddress}");
                Thread.Sleep(500);

                // 点击确认按钮
                double confirmX = 0.4;
                double confirmY = 0.49;
                int absConfirmX = (int)(windowRect.X + windowRect.Width * confirmX);
                int absConfirmY = (int)(windowRect.Y + windowRect.Height * confirmY);
                ClickAtPosition(absConfirmX, absConfirmY);
                Console.WriteLine("已点击确认按钮");
                Thread.Sleep(1000);
            }

            // === 3. 滚轮在坐标(x,y)向后缩4个度 ===
            int wheelX = (int)(windowRect.X + windowRect.Width * 0.56); // 示例x坐标
            int wheelY = (int)(windowRect.Y + windowRect.Height * 0.46); // 示例y坐标
            SetCursorPos(wheelX, wheelY);
            for (int i = 0; i < 4; i++)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -120, 0); // 向后滚动
                Thread.Sleep(100);
            }
            Console.WriteLine("已执行滚轮缩放操作");

            // === 4. 拖动鼠标从坐标A到坐标B ===
            int dragStartX = (int)(windowRect.X + windowRect.Width * 0.56);
            int dragStartY = (int)(windowRect.Y + windowRect.Height * 0.48);
            int dragEndX = (int)(windowRect.X + windowRect.Width * 0.56);
            int dragEndY = (int)(windowRect.Y + windowRect.Height * 0.45);

            MouseDrag(dragStartX, dragStartY, dragEndX, dragEndY);
            Console.WriteLine($"已从({dragStartX},{dragStartY})拖动到({dragEndX},{dragEndY})");
            Thread.Sleep(500);

            // === 5. 点击两个不同的按钮 ===
            // 第三个按钮点击
            double btn3X = 0.58;
            double btn3Y = 0.72;
            int absBtn3X = (int)(windowRect.X + windowRect.Width * btn3X);
            int absBtn3Y = (int)(windowRect.Y + windowRect.Height * btn3Y);
            ClickAtPosition(absBtn3X, absBtn3Y);
            Console.WriteLine("已点击第三个按钮");
            Thread.Sleep(500);

            // 第四个按钮点击
            double btn4X = 0.5;
            double btn4Y = 0.74;
            int absBtn4X = (int)(windowRect.X + windowRect.Width * btn4X);
            int absBtn4Y = (int)(windowRect.Y + windowRect.Height * btn4Y);
            ClickAtPosition(absBtn4X, absBtn4Y);
            Console.WriteLine("已点击第四个按钮");
            Thread.Sleep(500);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"准备直播时出错: {ex.Message}");
            return false;
        }

    }
    public bool HeadlessMode { get; set; } = false;
    public string ChromeDriverPath { get; set; } = @"C:\path\to\chromedriver";
    public string DouyinCookies { get; set; } = "your_cookie_string_here";
    public void Clickplaybacktask(ReplayVideo replayvideo, Task task)
    {
        // 使用类属性中的配置
        bool headlessMode = this.HeadlessMode;
        string chromeDriverPath = this.ChromeDriverPath;
        string douyinCookies = this.DouyinCookies;

        // Chrome 配置
        var options = new ChromeOptions();
        if (headlessMode)
        {
            options.AddArguments("--headless", "--disable-gpu");
        }
        options.AddArguments("--disable-infobars", "--disable-notifications");
        options.AddArgument("--lang=zh-CN");
        options.AddArgument("--window-size=1200,900"); // 设置窗口大小

        // 创建 Driver 服务
        var service = ChromeDriverService.CreateDefaultService(chromeDriverPath);
        service.HideCommandPromptWindow = true;
        service.Port = new Random().Next(64000, 65000);

        using (var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(60)))
        {
            Console.WriteLine("正在准备抖音回放上传浏览器...");
            driver.Manage().Window.Maximize();

            // 1. 先访问任意抖音页面建立域名上下文
            driver.Navigate().GoToUrl("https://www.douyin.com");
            Thread.Sleep(2000);

            // 2. 注入 Cookie
            Console.WriteLine("正在注入 Cookie...");
            var cookieDict = douyinCookies.Split(';')
                .Select(c => c.Trim().Split('='))
                .Where(c => c.Length == 2)
                .ToDictionary(c => c[0], c => c[1]);

            foreach (var cookie in cookieDict)
            {
                driver.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, ".douyin.com", "/", null));
            }

            // 3. 刷新使 Cookie 生效
            driver.Navigate().Refresh();
            Thread.Sleep(3000);

            // 4. 访问回放上传页面
            Console.WriteLine("正在访问抖音创作服务平台上传页面...");
            driver.Navigate().GoToUrl("https://creator.douyin.com/creator-micro/content/upload");
            Thread.Sleep(5000);

            // 5. 检查登录状态
            if (driver.Url.Contains("passport.douyin.com"))
            {
                throw new Exception("Cookie登录失败，请检查Cookie有效性");
            }

            // 6. 点击上传按钮
            Console.WriteLine("点击上传按钮...");
            var uploadButton = driver.FindElement(By.CssSelector("div.upload-btn"));
            uploadButton.Click();
            Thread.Sleep(2000);

            // 7. 输入视频地址
            Console.WriteLine($"输入视频地址: {task.VideoAddress}");
            var urlInput = driver.FindElement(By.CssSelector("input[placeholder='请输入作品地址']"));
            urlInput.Clear();
            urlInput.SendKeys(task.VideoAddress);
            Thread.Sleep(1000);

            // 8. 点击确认按钮
            Console.WriteLine("点击确认按钮...");
            var confirmButton = driver.FindElement(By.CssSelector("button.upload-web-btn"));
            confirmButton.Click();
            Thread.Sleep(5000); // 等待视频加载

            // 9. 输入标题
            Console.WriteLine($"输入视频标题: {replayvideo.WechatVideoTitle}");
            var titleInput = driver.FindElement(By.CssSelector("input[placeholder='填写标题，可能会有更多赞哦~']"));
            titleInput.Clear();
            titleInput.SendKeys(replayvideo.WechatVideoTitle);
            Thread.Sleep(1000);

            // 10. 输入视频描述
            Console.WriteLine($"输入视频描述: {replayvideo.WechatVideoDescription}");
            var descInput = driver.FindElement(By.CssSelector("textarea[placeholder='填写作品描述']"));
            descInput.Clear();
            descInput.SendKeys(replayvideo.WechatVideoDescription);
            Thread.Sleep(1000);

            // 11. 滚动到底部
            Console.WriteLine("滚动到页面底部...");
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
            Thread.Sleep(2000);

            // 12. 点击发布按钮
            Console.WriteLine("点击发布按钮...");
            var publishButton = driver.FindElement(By.CssSelector("button.publish-btn"));
            publishButton.Click();
            Thread.Sleep(3000);

            // 13. 确认发布（如果存在确认按钮）
            try
            {
                var confirmPublish = driver.FindElement(By.CssSelector("button.dialog-footer-button--primary"));
                if (confirmPublish.Displayed)
                {
                    Console.WriteLine("点击确认发布按钮...");
                    confirmPublish.Click();
                    Thread.Sleep(2000);
                }
            }
            catch
            {
                Console.WriteLine("未找到二次确认按钮，直接发布完成");
            }

            Console.WriteLine("回放视频上传完成!");
        }
    }

    // 添加鼠标拖动功能
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

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
    // 添加鼠标滚轮事件常量
    private const int MOUSEEVENTF_WHEEL = 0x0800;

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    public bool ClickEndLiveButton()
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }
            var windowRect = _window.BoundingRectangle;

            // 第一步：点击"结束直播"按钮
            double stopButtonRelativeX = 0.8;
            double stopButtonRelativeY = 0.93;
            int stopButtonX = (int)(windowRect.Width * stopButtonRelativeX);
            int stopButtonY = (int)(windowRect.Height * stopButtonRelativeY);
            int stopButtonAbsoluteX = (int)windowRect.X + stopButtonX;
            int stopButtonAbsoluteY = (int)windowRect.Y + stopButtonY;

            Console.WriteLine($"准备点击结束直播按钮位置: X={stopButtonAbsoluteX}, Y={stopButtonAbsoluteY}");
            ClickAtPosition(stopButtonAbsoluteX, stopButtonAbsoluteY);

            // 等待确认对话框出现
            Thread.Sleep(2000); // 等待1秒让确认对话框弹出

            // 获取最新窗口位置
            windowRect = _window.BoundingRectangle;

            // 第二步：点击确认按钮
            double confirmButtonRelativeX = 0.52;
            double confirmButtonRelativeY = 0.54;
            //double confirmButtonRelativeX = 0.45;
            //double confirmButtonRelativeY = 0.54;
            int confirmButtonX = (int)(windowRect.Width * confirmButtonRelativeX);
            int confirmButtonY = (int)(windowRect.Height * confirmButtonRelativeY);
            int confirmButtonAbsoluteX = (int)windowRect.X + confirmButtonX;
            int confirmButtonAbsoluteY = (int)windowRect.Y + confirmButtonY;

            Console.WriteLine($"准备点击确认结束按钮位置: X={confirmButtonAbsoluteX}, Y={confirmButtonAbsoluteY}");
            ClickAtPosition(confirmButtonAbsoluteX, confirmButtonAbsoluteY);
            Thread.Sleep(500);
            ClickAtPosition(confirmButtonAbsoluteX, confirmButtonAbsoluteY);
            Console.WriteLine("直播已成功结束!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击按钮时出错: {ex.Message}");
            return false;
        }

    }

    public class DouyinLiveSession
    {
        public string Title { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string CoverImageUrl { get; set; }
        public Dictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            return $"直播标题: {Title}\n" +
                   $"开播时间: {StartTime}\n" +
                   $"关播时间: {EndTime}\n" +
                   $"直播时长: {Duration}\n" +
                   $"封面URL: {CoverImageUrl}\n" +
                   $"指标数据: \n{string.Join("\n", Metrics.Select(kv => $"- {kv.Key}: {kv.Value}"))}";
        }
    }

    public List<DouyinLiveSession> ScrapeLiveData()
    {
        var results = new List<DouyinLiveSession>();

        // Chrome 配置
        var options = new ChromeOptions();
        if (HeadlessMode)
        {
            options.AddArguments("--headless", "--disable-gpu");
        }
        options.AddArguments("--disable-infobars", "--disable-notifications");
        options.AddArgument("--lang=zh-CN");
        options.AddArgument("--window-size=1200,900");

        // 创建 Driver 服务
        var service = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
        service.HideCommandPromptWindow = true;
        service.Port = new Random().Next(64000, 65000);

        try
        {
            using (var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30)))
            {
                Console.WriteLine("正在准备抖音数据爬取浏览器...");
                driver.Manage().Window.Maximize();

                // 1. 先访问任意抖音页面建立域名上下文
                driver.Navigate().GoToUrl("https://www.douyin.com");
                Thread.Sleep(2000);

                // 2. 注入 Cookie
                Console.WriteLine("正在注入 Cookie...");
                var cookieDict = DouyinCookies.Split(';')
                    .Select(c => c.Trim().Split('='))
                    .Where(c => c.Length == 2)
                    .ToDictionary(c => c[0], c => c[1]);

                foreach (var cookie in cookieDict)
                {
                    driver.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, ".douyin.com", "/", null));
                }

                // 3. 刷新使 Cookie 生效
                driver.Navigate().Refresh();
                Thread.Sleep(3000);

                // 4. 访问直播数据页面
                Console.WriteLine("正在访问直播数据页面...");
                driver.Navigate().GoToUrl("https://anchor.douyin.com/anchor/review?from=default");
                Thread.Sleep(8000); // 确保页面加载完成

                // 5. 检查登录状态
                if (driver.Url.Contains("passport.douyin.com"))
                {
                    throw new Exception("Cookie登录失败，请检查Cookie有效性");
                }

                // 6. 获取数据
                Console.WriteLine("正在解析直播数据...");
                results = ScrapeLiveSessionData(driver);

                // 7. 显示结果
                Console.WriteLine("\n========== 抖音直播数据统计 ==========");
                foreach (var session in results)
                {
                    Console.WriteLine(session);
                    Console.WriteLine("------------------------------------");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"爬取过程中发生错误: {ex.Message}");
        }

        return results;
    }

    private List<DouyinLiveSession> ScrapeLiveSessionData(IWebDriver driver)
    {
        var results = new List<DouyinLiveSession>();
        try
        {
            // 查找所有直播场次元素
            var sessionElements = driver.FindElements(By.CssSelector(".review-item.main-card-wrap"));

            foreach (var sessionElement in sessionElements)
            {
                try
                {
                    var session = new DouyinLiveSession();

                    // 提取基础数据
                    var basicSection = sessionElement.FindElement(By.CssSelector(".review-basic"));
                    session.CoverImageUrl = basicSection.FindElement(By.CssSelector(".basic-img")).GetAttribute("src");
                    session.Title = basicSection.FindElement(By.CssSelector(".basic-name")).Text;

                    // 提取时间数据
                    var timeElements = basicSection.FindElements(By.CssSelector(".basic-time-value"));
                    if (timeElements.Count >= 3)
                    {
                        session.StartTime = timeElements[0].Text;
                        session.EndTime = timeElements[1].Text;
                        session.Duration = timeElements[2].Text;
                    }

                    // 提取指标数据
                    var metricCards = sessionElement.FindElements(By.CssSelector(".card-template"));
                    foreach (var card in metricCards)
                    {
                        var cardTitle = card.FindElement(By.CssSelector(".indicator-card-title")).Text;
                        var items = card.FindElements(By.CssSelector(".indicator-card-item"));

                        foreach (var item in items)
                        {
                            try
                            {
                                var label = item.FindElement(By.CssSelector(".indicator-card-item-label")).Text;
                                string value = "";

                                // 尝试两种值获取方式
                                var popoverElements = item.FindElements(By.CssSelector(".indicator-card-item-value-popover"));
                                if (popoverElements.Count > 0)
                                {
                                    value = popoverElements[0].Text;
                                }
                                else
                                {
                                    var valueElements = item.FindElements(By.CssSelector(".indicator-card-item-value"));
                                    if (valueElements.Count > 0) value = valueElements[0].Text;
                                }

                                // 获取单位
                                var unitElements = item.FindElements(By.CssSelector(".indicator-card-item-unit"));
                                if (unitElements.Count > 0 && !value.EndsWith(unitElements[0].Text))
                                {
                                    value += unitElements[0].Text;
                                }

                                if (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(value))
                                {
                                    session.Metrics[$"{cardTitle}_{label}"] = value;
                                }
                            }
                            catch { /* 忽略单个指标错误 */ }
                        }
                    }

                    results.Add(session);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理直播场次时出错: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析直播数据时出错: {ex.Message}");
        }
        return results;
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
