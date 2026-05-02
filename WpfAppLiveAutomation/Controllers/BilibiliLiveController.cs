using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium.Support.UI;
public class BilibiliLiveController : LiveControllerBase
{
    protected override string ApplicationPath => @"D:\bilibililive\livehime\livehime.exe";
    protected override string WindowTitle => "哔哩哔哩直播姬"; 
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
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return;
            }

            var windowRect = _window.BoundingRectangle;
            int buttonX = (int)(windowRect.Width * 0.8);
            int buttonY = (int)(windowRect.Height * 0.91);
            int absoluteX = (int)windowRect.X + buttonX;
            int absoluteY = (int)windowRect.Y + buttonY;

            ClickAtPosition(absoluteX, absoluteY);
            Console.WriteLine($"成功点击按钮");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"点击按钮时出错: {ex.Message}");
        }
    }
    //public bool Clickpreparetask(string liveTopic)
    //{
    //    try
    //    {

    //        if (_window == null)
    //        {
    //            Console.WriteLine("窗口未初始化，无法点击按钮");
    //            return false;
    //        }

    //        var windowRect = _window.BoundingRectangle;
    //        int relativeX = (int)(windowRect.Width * 0.32);
    //        int relativeY = (int)(windowRect.Height * 0.05);
    //        int absoluteX = (int)windowRect.X + relativeX;
    //        int absoluteY = (int)windowRect.Y + relativeY;

    //        ClickAtPosition(absoluteX, absoluteY);
    //        Thread.Sleep(500); // 等待500ms确保界面刷新

    //        // 第二个点击位置
    //        int relativeX1 = (int)(windowRect.Width * 0.53);
    //        int relativeY1 = (int)(windowRect.Height * 0.41);
    //        int absoluteX1 = (int)windowRect.X + relativeX1;
    //        int absoluteY1 = (int)windowRect.Y + relativeY1;

    //        ClickAtPosition(absoluteX1, absoluteY1);
    //        Thread.Sleep(500); // 再次等待

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
    //        // 标题长度验证
    //        if (string.IsNullOrEmpty(topicText) || topicText.Length < 2 || topicText.Length > 30)
    //        {
    //            Console.WriteLine("标题长度需在2-30字符之间");
    //            return false;
    //        }

    //        var windowRect = _window.BoundingRectangle;
    //        // 哔哩哔哩标题输入框相对位置（根据实际UI调整）
    //        double titleBoxX = 0.53;
    //        double titleBoxY = 0.41;
    //        int absX = windowRect.X + (int)(windowRect.Width * titleBoxX);
    //        int absY = windowRect.Y + (int)(windowRect.Height * titleBoxY);

    //        ClickAtPosition(absX, absY);

    //        // 清空并输入新标题
    //        ClearAndInputText(topicText);
    //        Console.WriteLine($"已输入标题: {topicText}");
    //        Thread.Sleep(500);

    //        int relativeX = (int)(windowRect.Width * 0.6);
    //        int relativeY = (int)(windowRect.Height * 0.22);
    //        int absoluteX = (int)windowRect.X + relativeX;
    //        int absoluteY = (int)windowRect.Y + relativeY;

    //        ClickAtPosition(absoluteX, absoluteY);
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"填写标题出错: {ex.Message}");
    //        return false;
    //    }
    //}
    public bool Clickpreparetask(string liveTopic,Task task)
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }

            // 标题长度验证
            if (string.IsNullOrEmpty(liveTopic) || liveTopic.Length < 2 || liveTopic.Length > 30)
            {
                Console.WriteLine("标题长度需在2-30字符之间");
                return false;
            }

            var windowRect = _window.BoundingRectangle;

            // === 三步点击流程 ===
            // 1. 主按钮点击
            double point1X = 0.25;
            double point1Y = 0.07;
            int absX1 = (int)(windowRect.X + windowRect.Width * point1X);
            int absY1 = (int)(windowRect.Y + windowRect.Height * point1Y);
            ClickAtPosition(absX1, absY1);
            ClickAtPosition(529, 73);  // 使用绝对坐标替代相对坐标
            Thread.Sleep(800);



            // === 标题输入流程 ===
            // 设置英文输入法避免干扰
            SetEnglishKeyboardLayout();
            Thread.Sleep(200);

            // 清空并输入直播标题
            ClearAndInputText(liveTopic);
            Console.WriteLine($"已输入标题: {liveTopic}");
            Thread.Sleep(800);  // 确保输入完成


            // === 1. 点击两个不同的按钮 ===
            // 第一个按钮点击 (示例坐标，请根据实际UI调整)
            double btn1X = 0.18;
            double btn1Y = 0.08;
            int absBtn1X = (int)(windowRect.X + windowRect.Width * btn1X);
            int absBtn1Y = (int)(windowRect.Y + windowRect.Height * btn1Y);
            ClickAtPosition(absBtn1X, absBtn1Y);
            Console.WriteLine("已点击第一个按钮");
            Thread.Sleep(5000);

            // 第二个按钮点击
            double btn2X = 0.53;
            double btn2Y = 0.46;
            int absBtn2X = (int)(windowRect.X + windowRect.Width * btn2X);
            int absBtn2Y = (int)(windowRect.Y + windowRect.Height * btn2Y);
            ClickAtPosition(absBtn2X, absBtn2Y);
            Console.WriteLine("已点击第二个按钮");
            Thread.Sleep(1000);

            // === 2. 在文件选择器中输入 tasks.PosterAddress 并点击确认 ===
            if (!string.IsNullOrEmpty(task.PosterAddress))
            {
                // 点击文件选择按钮
                double fileSelectX = 0.5;
                double fileSelectY = 0.64;
                int absFileSelectX = (int)(windowRect.X + windowRect.Width * fileSelectX);
                int absFileSelectY = (int)(windowRect.Y + windowRect.Height * fileSelectY);
                ClickAtPosition(absFileSelectX, absFileSelectY);
                Thread.Sleep(1000);

                // 输入海报地址
                ClearAndInputText(task.PosterAddress);
                Console.WriteLine($"已输入海报地址: {task.PosterAddress}");
                Thread.Sleep(500);

                // 点击确认按钮
                double confirmX = 0.74;
                double confirmY = 0.675;
                int absConfirmX = (int)(windowRect.X + windowRect.Width * confirmX);
                int absConfirmY = (int)(windowRect.Y + windowRect.Height * confirmY);
                ClickAtPosition(absConfirmX, absConfirmY);
                Console.WriteLine("已点击确认按钮");
                Thread.Sleep(3000);
            }

            // === 3. 拖动鼠标从坐标A到坐标B ===
            int dragStartAX = (int)(windowRect.X + windowRect.Width * 0.45);
            int dragStartAY = (int)(windowRect.Y + windowRect.Height * 0.47);
            int dragEndBX = (int)(windowRect.X + windowRect.Width * 0.45);
            int dragEndBY = (int)(windowRect.Y + windowRect.Height * 0.41);
            MouseDrag(dragStartAX, dragStartAY, dragEndBX, dragEndBY);
            Console.WriteLine($"已完成第一次拖动: ({dragStartAX},{dragStartAY}) -> ({dragEndBX},{dragEndBY})");
            Thread.Sleep(3000);

            // === 4. 拖动鼠标从坐标C到坐标D ===
            //int dragStartCX = (int)(windowRect.X + windowRect.Width * 0.405);
            //int dragStartCY = (int)(windowRect.Y + windowRect.Height * 0.41);
            //int dragEndDX = (int)(windowRect.X + windowRect.Width * 0.395);
            //int dragEndDY = (int)(windowRect.Y + windowRect.Height * 0.41);
            //MouseDrag(dragStartCX, dragStartCY, dragEndDX, dragEndDY);
            //Console.WriteLine($"已完成第二次拖动: ({dragStartCX},{dragStartCY}) -> ({dragEndDX},{dragEndDY})");
            //Thread.Sleep(3000);

            //// === 5. 拖动鼠标从坐标E到坐标F ===
            //int dragStartEX = (int)(windowRect.X + windowRect.Width * 0.455);
            //int dragStartEY = (int)(windowRect.Y + windowRect.Height * 0.41);
            //int dragEndFX = (int)(windowRect.X + windowRect.Width * 0.498);
            //int dragEndFY = (int)(windowRect.Y + windowRect.Height * 0.41);
            //MouseDrag(dragStartEX, dragStartEY, dragEndFX, dragEndFY);
            //Console.WriteLine($"已完成第三次拖动: ({dragStartEX},{dragStartEY}) -> ({dragEndFX},{dragEndFY})");
            //Thread.Sleep(10000);

            // === 6. 点击两个不同的按钮 ===
            // 第三个按钮点击
            double btn3X = 0.64;
            double btn3Y = 0.76;
            int absBtn3X = (int)(windowRect.X + windowRect.Width * btn3X);
            int absBtn3Y = (int)(windowRect.Y + windowRect.Height * btn3Y);
            ClickAtPosition(absBtn3X, absBtn3Y);
            Console.WriteLine("已点击第三个按钮");
            Thread.Sleep(3000);

            // 第四个按钮点击
            double btn4X = 0.66;
            double btn4Y = 0.23;
            int absBtn4X = (int)(windowRect.X + windowRect.Width * btn4X);
            int absBtn4Y = (int)(windowRect.Y + windowRect.Height * btn4Y);
            ClickAtPosition(absBtn4X, absBtn4Y);
            Console.WriteLine("已点击第四个按钮");
            Thread.Sleep(3000);



            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"开播流程出错: {ex.Message}");
            return false;
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
    public bool Clickplaybacktask(string liveTopic)
    {
        try
        {
            if (_window == null)
            {
                Console.WriteLine("窗口未初始化，无法点击按钮");
                return false;
            }

            // 标题长度验证
            if (string.IsNullOrEmpty(liveTopic) || liveTopic.Length < 2 || liveTopic.Length > 30)
            {
                Console.WriteLine("标题长度需在2-30字符之间");
                return false;
            }

            var windowRect = _window.BoundingRectangle;

            // 1. 关闭"结束直播"弹窗按钮
            double point1X = 0.66;
            double point1Y = 0.27;
            int absX1 = (int)(windowRect.X + windowRect.Width * point1X);
            int absY1 = (int)(windowRect.Y + windowRect.Height * point1Y);
            ClickAtPosition(absX1, absY1);
            Thread.Sleep(800);

            // 2. 直播回放按钮点击
            double point2X = 0.12;
            double point2Y = 0.85;
            int absX2 = (int)(windowRect.X + windowRect.Width * point2X);
            int absY2 = (int)(windowRect.Y + windowRect.Height * point2Y);
            ClickAtPosition(absX2, absY2);
            Thread.Sleep(800);

            // 3. 直播回放按钮再次点击
            double point3X = 0.65;
            double point3Y = 0.29;
            int absX3 = (int)(windowRect.X + windowRect.Width * point3X);
            int absY3 = (int)(windowRect.Y + windowRect.Height * point3Y);
            ClickAtPosition(absX3, absY3);
            Thread.Sleep(1500);

            // 4. 标题输入框位置
            double textBoxX = 0.23;
            double textBoxY = 0.2;
            int absTextX = (int)(windowRect.X + windowRect.Width * textBoxX);
            int absTextY = (int)(windowRect.Y + windowRect.Height * textBoxY);
            ClickAtPosition(absTextX, absTextY);
            Thread.Sleep(500);

            // === 标题输入流程 ===
            // 设置英文输入法避免干扰
            SetEnglishKeyboardLayout();
            Thread.Sleep(200);

            // 清空并输入直播标题
            ClearAndInputText(liveTopic);
            Console.WriteLine($"已输入标题: {liveTopic}");
            Thread.Sleep(800);  // 确保输入完成

            // === 鼠标拖动进度条 ===
            // 起始位置 (0.45, 0.81)
            int startX = (int)(windowRect.X + windowRect.Width * 0.45);
            int startY = (int)(windowRect.Y + windowRect.Height * 0.8);

            // 结束位置 (0.64, 0.8)
            int endX = (int)(windowRect.X + windowRect.Width * 0.64);
            int endY = (int)(windowRect.Y + windowRect.Height * 0.8);

            // 执行拖动操作
            MouseDrag(startX, startY, endX, endY);
            Thread.Sleep(500);  // 等待拖动完成

            // === 确认按钮点击 ===
            double confirmX = 0.78;
            double confirmY = 0.19;
            int absConfirmX = (int)(windowRect.X + windowRect.Width * confirmX);
            int absConfirmY = (int)(windowRect.Y + windowRect.Height * confirmY);
            ClickAtPosition(absConfirmX, absConfirmY);
            Thread.Sleep(1000);  // 等待操作生效

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"开播流程出错: {ex.Message}");
            return false;
        }
    }
    public void ScrapeLiveData(bool headlessMode, string chromeDriverPath, int clickX, int clickY, string bilibiliCookies)
    {
        try
        {
            // Chrome配置
            var options = new ChromeOptions();
            if (headlessMode) options.AddArguments("--headless", "--disable-gpu");
            options.AddArguments("--disable-infobars", "--disable-notifications");

            // 创建Driver服务
            var service = ChromeDriverService.CreateDefaultService(chromeDriverPath);
            service.HideCommandPromptWindow = true;
            service.Port = new Random().Next(64000, 65000);

            using (var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30)))
            {
                Console.WriteLine("正在准备浏览器...");
                driver.Manage().Window.Maximize();

                // 建立域名上下文
                driver.Navigate().GoToUrl("https://www.bilibili.com");
                Thread.Sleep(1000);

                // 注入Cookie
                Console.WriteLine("正在注入Cookie...");
                var cookieDict = bilibiliCookies.Split(';')
                    .Select(c => c.Trim().Split('='))
                    .Where(c => c.Length == 2)
                    .ToDictionary(c => c[0], c => c[1]);

                foreach (var cookie in cookieDict)
                {
                    driver.Manage().Cookies.AddCookie(new Cookie(cookie.Key, cookie.Value, ".bilibili.com", "/", null));
                }

                // 刷新使Cookie生效
                driver.Navigate().Refresh();
                Thread.Sleep(2000);

                // 第一步：访问总览页面并爬取数据
                Console.WriteLine("正在访问总览页面...");
                driver.Navigate().GoToUrl("https://link.bilibili.com/p/center/index#/live-data/overview");
                Thread.Sleep(5000); // 等待页面加载

                // 检查登录状态
                if (driver.Url.Contains("passport.bilibili.com"))
                {
                    throw new Exception("Cookie登录失败，请检查Cookie有效性");
                }

                // 爬取总览页面数据
                Console.WriteLine("正在解析总览数据...");
                var overviewResults = ScrapeOverviewData(driver);
                PrintResults("总览数据", overviewResults);

                // 第二步：访问场次数据页面
                Console.WriteLine("正在访问场次数据页面...");
                driver.Navigate().GoToUrl("https://link.bilibili.com/p/center/index#/live-data/session-data");
                Thread.Sleep(5000); // 等待页面加载

                // 使用坐标点击场次
                Console.WriteLine($"正在点击坐标 ({clickX}, {clickY})...");
                new OpenQA.Selenium.Interactions.Actions(driver)
                    .MoveByOffset(clickX, clickY)
                    .Click()
                    .Perform();
                Thread.Sleep(3000); // 等待详情加载

                // 获取场次详情数据
                Console.WriteLine("正在解析场次详情数据...");
                var sessionResults = ScrapeSessionData(driver);
                PrintResults("场次详情数据", sessionResults);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"爬取过程中发生错误: {ex.Message}");
        }
    }

    // 总览页面数据爬取
    private List<LiveDataItem> ScrapeOverviewData(IWebDriver driver)
    {
        var results = new List<LiveDataItem>();
        var cards = driver.FindElements(By.CssSelector(".data-card"));

        foreach (var card in cards)
        {
            try
            {
                var name = card.FindElement(By.CssSelector(".name")).Text;
                var value = card.FindElement(By.CssSelector(".num")).Text;
                var unit = card.FindElement(By.CssSelector(".unit"))?.Text ?? "";

                string change = "无变化";
                try
                {
                    var ratioDiv = card.FindElement(By.CssSelector(".data-ratio"));
                    var ratio = ratioDiv.FindElement(By.CssSelector(".ratio")).Text;
                    var ratioUnit = ratioDiv.FindElement(By.CssSelector(".ratio-unit"))?.Text ?? "";
                    var angle = ratioDiv.FindElement(By.CssSelector(".angle"));
                    var changeType = angle.GetAttribute("class").Contains("down") ? "下降" : "上升";
                    change = $"{changeType} {ratio}{ratioUnit}";
                }
                catch { /* 忽略没有变化率的情况 */ }

                results.Add(new LiveDataItem(name, $"{value}{unit}", change));
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理卡片时出错: {ex.Message}");
            }
        }

        return results;
    }

    // 场次详情数据爬取
    private Dictionary<string, string> ScrapeSessionData(IWebDriver driver)
    {
        var results = new Dictionary<string, string>();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        try
        {
            // 获取基础信息
            var sessionHeader = wait.Until(d => d.FindElement(By.CssSelector(".session-header")));
            results["直播标题"] = sessionHeader.FindElement(By.CssSelector(".title")).Text;
            results["直播时间"] = sessionHeader.FindElement(By.CssSelector(".time")).Text;
            results["直播时长"] = sessionHeader.FindElement(By.XPath(".//span[contains(text(),'直播时长')]")).Text.Split('：')[1];

            // 获取数据卡片
            var cards = driver.FindElements(By.CssSelector(".data-card"));
            foreach (var card in cards)
            {
                var name = card.FindElement(By.CssSelector(".name")).Text;
                var value = card.FindElement(By.CssSelector(".num")).Text;
                var unit = card.FindElement(By.CssSelector(".unit"))?.Text ?? "";
                results[name] = $"{value}{unit}";
            }

            // 获取观众贡献排行状态
            var rankSections = driver.FindElements(By.CssSelector(".rank-item-list"));
            if (rankSections.Count >= 2)
            {
                results["点赞送礼"] = rankSections[0].FindElement(By.CssSelector(".list-items-null"))?.Text ?? "无数据";
                results["首次送礼"] = rankSections[1].FindElement(By.CssSelector(".list-items-null"))?.Text ?? "无数据";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析数据时出错: {ex.Message}");
        }
        return results;
    }

    // 打印结果
    private void PrintResults(string title, object results)
    {
        Console.WriteLine($"\n========== {title} ==========");

        if (results is List<LiveDataItem> listResults)
        {
            foreach (var item in listResults)
            {
                Console.WriteLine($"{item.MetricName,-15}: {item.Value,-15} ({item.Change})");
            }
        }
        else if (results is Dictionary<string, string> dictResults)
        {
            foreach (var item in dictResults)
            {
                Console.WriteLine($"{item.Key,-20}: {item.Value}");
            }
        }
    }

    // 直播数据项内部类
    private class LiveDataItem
    {
        public string MetricName { get; }
        public string Value { get; }
        public string Change { get; }

        public LiveDataItem(string name, string value, string change)
        {
            MetricName = name;
            Value = value;
            Change = change;
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

    [Flags]
    private enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        Absolute = 0x00008000
    }

    [DllImport("user32.dll")]
    private static extern void MouseEvent(MouseEventFlags flags, int dx, int dy, uint data, UIntPtr extraInfo);

}
