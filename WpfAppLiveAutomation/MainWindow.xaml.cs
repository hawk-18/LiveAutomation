// MainWindow.xaml.cs
using Microsoft.Data.Sqlite;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace WpfAppLiveAutomation;
using System.Collections.Concurrent;

    public partial class MainWindow : Window
    {

        private DispatcherTimer _taskSchedulerTimer;
        private HashSet<int> _preparedTasks = new HashSet<int>();
        private HashSet<int> _startedTasks = new HashSet<int>();
        private HashSet<int> _endedTasks = new HashSet<int>();
        private HashSet<int> _replayedTasks = new HashSet<int>();
        private bool _obsPrepared = false;

        private WeChatLiveController _wechatController = new WeChatLiveController();
        private BilibiliLiveController _bilibiliController = new BilibiliLiveController();
        private DouyinLiveController _douyinController = new DouyinLiveController();
        private OBSController _obsController = new OBSController();
        DelayHelper delay = new DelayHelper();
        private HashSet<int> _scrapedTasks = new HashSet<int>();
        private ConcurrentQueue<Task> _scrapingQueue = new ConcurrentQueue<Task>();
        private bool _isScrapingThreadActive = false;
        private Thread _scrapingThread;
        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            LoadTasks();
            InitializeTaskScheduler();
            _scrapingThread = new Thread(ScrapingWorker);
            _scrapingThread.IsBackground = true;
            _scrapingThread.Start();
        }

        private void InitializeTaskScheduler()
        {
            _taskSchedulerTimer = new DispatcherTimer();
            _taskSchedulerTimer.Interval = TimeSpan.FromSeconds(1);//每秒检查一次
            _taskSchedulerTimer.Tick += TaskSchedulerTimer_Tick;
            _taskSchedulerTimer.Start();
        }

        private void TaskSchedulerTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var allTasks = DatabaseHelper.GetAllTasks();
            // ==== 新增：检查过期任务 
            var expiredTasks = allTasks
                .Where(t => DateTime.TryParseExact(t.EndTime, "yyyy-MM-dd HH:mm:ss",
                         CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime) &&
                         now > endTime.AddHours(5) &&
                         t.Status != "Aborted" &&
                         t.Status != "Completed")
                .ToList();

            foreach (var task in expiredTasks)
            {
                try
                {
                    System.Console.WriteLine($"标记过期任务 '{task.TaskName}' 为已删除");
                    DatabaseHelper.DeleteTask(task.Id);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"处理过期任务失败: {ex.Message}");
                }
            }

            Dispatcher.Invoke(() =>
            {
                LoadTasks();
            });
            // 1. 准备阶段：使用用户设置的准备时间
            var preparationTasks = allTasks
                .Where(t => t.Status == "Pending" &&
                        DateTime.TryParseExact(t.PreparationTime, "yyyy-MM-dd HH:mm:ss",
                                 CultureInfo.InvariantCulture, DateTimeStyles.None, out var prepTime) &&
                        DateTime.TryParseExact(t.StartTime, "yyyy-MM-dd HH:mm:ss",
                                 CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime) &&
                        now >= prepTime &&
                        now < startTime)
                .ToList();

            foreach (var task in preparationTasks)
            {
                if (!_preparedTasks.Contains(task.Id))
                {
                    try
                    {
                        System.Console.WriteLine($"准备任务 '{task.TaskName}'...");
                        // ==== 新增：在准备开始时更新状态为 Executing ====
                        DatabaseHelper.UpdateTaskStatus(task.Id, "Executing");
                        // OBS准备操作
                        PrepareOBS(task);
                        SetMaxVolume();
                        Thread.Sleep(delay.GetRecommendedWaitTime("app_launch"));
                        // 平台特定准备操作
                        PreparePlatform(task);

                        System.Console.WriteLine($"任务 '{task.TaskName}' 准备完成");
                        _preparedTasks.Add(task.Id);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"准备任务 '{task.TaskName}' 时出错: {ex.Message}");
                    }
                }
            }

            // 2. 开播时间执行开播操作
            var pendingTasks = allTasks
                .Where(t => t.Status == "Executing" &&
                        DateTime.TryParseExact(t.StartTime, "yyyy-MM-dd HH:mm:ss",
                                 CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime) &&
                        now >= startTime)
                .ToList();

            foreach (var task in pendingTasks)
            {
                if (!_startedTasks.Contains(task.Id))
                {
                    try
                    {
                        ExecuteTaskStart(task);
                        _startedTasks.Add(task.Id); // 标记为已开播
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"开播失败: {ex.Message}");
                    }
                }
            }


            // 3. 关播时间执行关播操作
            var executingTasks = allTasks
                .Where(t => t.Status == "Executing" &&
                        DateTime.TryParseExact(t.EndTime, "yyyy-MM-dd HH:mm:ss",
                                 CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime) &&
                        now >= endTime)
                .ToList();

            foreach (var task in executingTasks)
            {
                if (!_endedTasks.Contains(task.Id))
                {
                    try
                    {
                        ExecuteTaskEnd(task);
                        DatabaseHelper.UpdateTaskStatus(task.Id, "Completed");
                        // 清理状态记录
                        _preparedTasks.Remove(task.Id);
                        _startedTasks.Remove(task.Id);
                        _endedTasks.Add(task.Id); // 标记为已关播
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"关播失败: {ex.Message}");
                    }
                }
            }    // 最终刷新界面
            Dispatcher.Invoke(() => {
                LoadTasks();
                // 强制刷新DataGrid
                executingTasksGrid.Items.Refresh();
                pendingTasksGrid.Items.Refresh();
                completedTasksGrid.Items.Refresh();
            });


            // 4. 关播时间后执行回放视频操作
            var Replaytask = allTasks
                .Where(t => DateTime.TryParseExact(t.EndTime, "yyyy-MM-dd HH:mm:ss",
                               CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime) &&
                       now > endTime.AddMinutes(1) && now < endTime.AddMinutes(40) &&
                        !string.IsNullOrEmpty(t.VideoMD5))
                .ToList();


            foreach (var task in Replaytask)
            {
                if (!_replayedTasks.Contains(task.Id))
                {
                    try
                    {
                        ExecuteTaskReplay(task);
                        _replayedTasks.Add(task.Id); // 标记为已回放
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"回放失败: {ex.Message}");
                    }
                }
            }
            Dispatcher.Invoke(() => {
                LoadTasks();
                // 强制刷新DataGrid
                executingTasksGrid.Items.Refresh();
                pendingTasksGrid.Items.Refresh();
                completedTasksGrid.Items.Refresh();
            });
        // 5. 直播结束后执行数据爬取（直播结束60-90分钟后）
        var scrapingTasks = allTasks
            .Where(t => _endedTasks.Contains(t.Id) &&
                    !_scrapedTasks.Contains(t.Id) &&
                    DateTime.TryParseExact(t.EndTime, "yyyy-MM-dd HH:mm:ss",
                             CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime) &&
                    now > endTime.AddMinutes(60) &&
                    now < endTime.AddMinutes(90))
            .ToList();

        foreach (var task in scrapingTasks)
        {
            try
            {
                // 将任务加入爬取队列
                _scrapingQueue.Enqueue(task);
                _scrapedTasks.Add(task.Id);
                System.Console.WriteLine($"已加入爬取队列: {task.TaskName}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加入爬取队列失败: {ex.Message}");
            }
        }

    }

        private void PrepareOBS(Task task)
        {
            if (!_obsPrepared)
            {
                var obsController = new OBSController();
                obsController.OpenAndActivate();// 激活OBS
                obsController.ClickprepareLiveButton(task.TaskName,task.VideoAddress);// 输入任务名称和视频地址
                Thread.Sleep(delay.GetRecommendedWaitTime("window_activate"));
                obsController.ClickButton1(); // 激活屏幕并暂停播放
                Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                obsController.ClickButton2(); // 启动虚拟摄像头
                _obsPrepared = true;
                System.Console.WriteLine("OBS准备完成");
            }
        }
        
        private void PreparePlatform(Task task)
        {
            foreach (var platform in task.Platform
                .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()))
            {
                switch (platform)
                {
                    case "哔哩哔哩":
                        var bilibiliController = new BilibiliLiveController();
                        bilibiliController.OpenAndActivate();// 激活哔哩哔哩
                        Thread.Sleep(delay.GetRecommendedWaitTime("app_launch"));
                        bilibiliController.Clickpreparetask(task.LiveTitle,task);// 输入直播间标题等
                        Thread.Sleep(delay.GetRecommendedWaitTime("button_click "));
                        System.Console.WriteLine("哔哩哔哩准备完成");
                        break;
                    case "抖音":
                        var douyinController = new DouyinLiveController();
                        douyinController.OpenAndActivate();
                        Thread.Sleep(delay.GetRecommendedWaitTime("app_launch"));
                        douyinController.ClickPrepareLiveButton(task.LiveTitle,task);// 输入直播间标题等
                        System.Console.WriteLine("抖音准备完成");
                        Thread.Sleep(delay.GetRecommendedWaitTime("task_interval"));
                        break;
                    case "微信视频号":
                        var wechatController = new WeChatLiveController();
                        wechatController.OpenAndActivate();// 激活微信
                        Thread.Sleep(delay.GetRecommendedWaitTime("app_launch"));
                        wechatController.ClickWeChatButtons();// 点击视频号按钮
                        Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                        wechatController.ActivateLiveToolWindow();// 激活直播工具窗口
                        Thread.Sleep(delay.GetRecommendedWaitTime("window_activate"));
                        wechatController.ClickStartLiveButton(task.LiveTitle,task);//输入直播间标题等
                        Thread.Sleep(delay.GetRecommendedWaitTime("screen_capture"));
                        System.Console.WriteLine("微信视频号准备完成");
                        break;
                }
            }
        }

        private void ExecuteTaskStart(Task task)
        {
            try
            {
                System.Console.WriteLine($"开始执行任务 '{task.TaskName}'...");
                var obsController = new OBSController();
                // OBS开播操作
                obsController.OpenAndActivate();
                obsController.ClickButton3(); // 开始直播
                Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                // 平台开播操作
                foreach (var platform in task.Platform
                 .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(p => p.Trim()))
                 {
                    switch (platform)
                    {

                        case "微信视频号":
                            var wechatController = new WeChatLiveController();
                            wechatController.OpenAndActivate();
                            wechatController.ActivateLiveToolWindow();// 激活直播工具窗口
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            wechatController.StartLiveBroadcast();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            break;

                        case "哔哩哔哩":
                            var bilibiliController = new BilibiliLiveController();
                            bilibiliController.OpenAndActivate();
                            Thread.Sleep(delay.GetRecommendedWaitTime("window_activation"));
                            bilibiliController.ClickStartLiveButton();
                            Thread.Sleep(delay.GetRecommendedWaitTime("stream_start"));
                            break;

                        case "抖音":
                            var douyinController = new DouyinLiveController();
                            douyinController.OpenAndActivate();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            douyinController.ClickStartLiveButton();
                            break;
                    }
                }
                System.Console.WriteLine($"任务 '{task.TaskName}' 已开始执行");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"启动任务失败: {ex.Message}");
            }
        }

        private void ExecuteTaskEnd(Task task)
        {
            try
            {
                System.Console.WriteLine($"结束任务 '{task.TaskName}'...");
                
                foreach (var platform in task.Platform
                    .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()))
                {
                    switch (platform)
                    {
                        case "微信视频号":
                            var wechatController = new WeChatLiveController();
                            wechatController.OpenAndActivate();
                            wechatController.ActivateLiveToolWindow();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            wechatController.StopLiveBroadcast();
                            break;

                        case "哔哩哔哩":
                            var bilibiliController = new BilibiliLiveController();
                            bilibiliController.OpenAndActivate();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            bilibiliController.ClickStartLiveButton();//与开播同一个按钮
                            break;

                        case "抖音":
                            var douyinController = new DouyinLiveController();
                            douyinController.OpenAndActivate();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            douyinController.ClickEndLiveButton();
                            break;
                    }
                }
                var obsController = new OBSController();
                // OBS关播操作
                obsController.OpenAndActivate();
                obsController.ClickButton3(); // 停止直播
                obsController.ClickButton2(); // 停止虚拟摄像头
                System.Console.WriteLine($"任务 '{task.TaskName}' 已结束");
                _preparedTasks.Remove(task.Id);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"结束任务失败: {ex.Message}");
            }
        }

        private void ExecuteTaskReplay(Task task)
        {
            try
            {
                // 检查VideoMD5是否唯一
                if (!DatabaseHelper.IsVideoMd5Unique(task.VideoMD5))
                {
                    System.Console.WriteLine($"任务 '{task.TaskName}' 的视频MD5不唯一，跳过回放");
                    return;
                }

                System.Console.WriteLine($"回放任务 '{task.TaskName}'...");
                var replayVideo = DatabaseHelper.GetReplayVideoByUnionId(task.UnionId);

                foreach (var platform in task.Platform
                    .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim()))
                {
                    switch (platform)
                    {
                        case "微信视频号":
                            var wechatController = new WeChatLiveController();
                            wechatController.OpenAndActivate();
                            wechatController.ActivateLiveToolWindow();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            wechatController.StartplaybacktaskBroadcast();
                            Thread.Sleep(delay.GetRecommendedWaitTime("window_activation"));
                            break;

                        case "哔哩哔哩":
                            var bilibiliController = new BilibiliLiveController();
                            if (replayVideo != null)
                            {
                                bilibiliController.OpenAndActivate();
                                Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                                bilibiliController.Clickplaybacktask(replayVideo.BilibiliTitle);
                                Thread.Sleep(delay.GetRecommendedWaitTime("window_activation"));
                            }
                            break;

                        case "抖音":
                            var douyinController = new DouyinLiveController();
                            douyinController.OpenAndActivate();
                            Thread.Sleep(delay.GetRecommendedWaitTime("button_click"));
                            douyinController.Clickplaybacktask(replayVideo, task);
                            break;
                    }
                }
                System.Console.WriteLine($"任务 '{task.TaskName}' 已回放");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"回放失败: {ex.Message}");
            }
        }


        private void LoadTasks()
        {
            var allTasks = DatabaseHelper.GetAllTasks();

            // 正在执行任务：状态为Executing
            var executingTasks = allTasks
                .Where(t => t.Status == "Executing")
                .ToList();

            // 使用精确到秒的时间解析进行排序
            var pendingTasks = allTasks
                .Where(t => t.Status == "Pending")
                .OrderBy(t => {  // 改为 OrderBy 升序排列
                    if (DateTime.TryParseExact(t.PreparationTime, "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                    {
                        return time;
                    }
                    return DateTime.MaxValue; // 解析失败的仍然排最后
                })
                .Take(20)
                .ToList();

            // 已执行任务：状态为Completed

            var completedTasks = allTasks
                .Where(t => t.Status == "Completed")
                .OrderByDescending(t => {
                    if (DateTime.TryParseExact(t.PreparationTime, "yyyy-MM-dd HH:mm:ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                    {
                        return time;
                    }
                    return DateTime.MaxValue;
                })
                .Take(20)
                .ToList();

            // 设置数据源
            executingTasksGrid.ItemsSource = executingTasks;
            pendingTasksGrid.ItemsSource = pendingTasks;
            completedTasksGrid.ItemsSource = completedTasks;
        }



        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow();

            if (addTaskWindow.ShowDialog() == true)
            {
                //DatabaseHelper.AddTask(addTaskWindow.NewTask);
                LoadTasks();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Task selectedTask = pendingTasksGrid.SelectedItem as Task ?? completedTasksGrid.SelectedItem as Task;
            if (selectedTask != null)
            {
                var editWindow = new EditTaskWindow(selectedTask);
                if (editWindow.ShowDialog() == true)
                {
                    // 创建新版本任务（逻辑新增）
                    Task newVersion = new Task()
                    {
                        // ==== 关键修改：继承原始UnionId ====
                        UnionId = selectedTask.UnionId,

                        // 保留原始创建信息
                        Creator = selectedTask.Creator,
                        CreateTime = selectedTask.CreateTime,

                        // 复制其他编辑后的字段
                        TaskName = editWindow.EditedTask.TaskName,
                        StartTime = editWindow.EditedTask.StartTime,
                        EndTime = editWindow.EditedTask.EndTime,
                        LiveTitle = editWindow.EditedTask.LiveTitle,
                        Status = "Pending", // 重置为待执行状态
                        Platform = editWindow.EditedTask.Platform,
                        VideoAddress = editWindow.EditedTask.VideoAddress,
                        PosterAddress = editWindow.EditedTask.PosterAddress,
                        PreparationTime = editWindow.EditedTask.PreparationTime,
                        
                        // 设置修改信息
                        Editor = editWindow.EditedTask.Editor,
                        EditorTime = editWindow.EditedTask.EditorTime,

                        // 关键关联字段
                        OriginalId = selectedTask.Id, // 指向原始记录
                        IsDeleted = 0 // 新记录有效
                    };

                    // 插入新版本记录
                    DatabaseHelper.AddTask(newVersion);

                    // 标记原始记录为已删除
                    selectedTask.IsDeleted = 1;
                    DatabaseHelper.UpdateTask(selectedTask);

                    // 刷新界面
                    LoadTasks();
                }
            }
        }


    const int clickX = 500;
    const int clickY = 300;

    private void ScrapingWorker()
    {
        while (true)
        {
            if (_scrapingQueue.TryDequeue(out Task task))
            {
                _isScrapingThreadActive = true;
                try
                {
                    System.Console.WriteLine($"开始爬取任务 '{task.TaskName}' 的直播数据...");

                    foreach (var platform in task.Platform
                     .Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(p => p.Trim()))
                    {
                        switch (platform)
                        {
                            case "抖音":
                                var douyinController = new DouyinLiveController();
                                // 属性赋值 + 无参调用
                                douyinController.HeadlessMode = true;
                                douyinController.ChromeDriverPath = @"C:\chromedriver\chromedriver.exe";
                                douyinController.DouyinCookies = "your_douyin_cookies_here";
                                var douyinData = douyinController.ScrapeLiveData();
                                DatabaseHelper.SaveScrapedData(task.Id, "抖音", douyinData);
                                break;

                            case "哔哩哔哩":
                                var bilibiliController = new BilibiliLiveController();
                                // 位置参数调用（移除命名参数）
                                bilibiliController.ScrapeLiveData(
                                    true,
                                    @"C:\chromedriver\chromedriver.exe",
                                    500,
                                    300,
                                    "your_bilibili_cookies_here"
                                );
                                break;
                        }
                    }

                    // 更新UI显示爬取完成
                    Dispatcher.Invoke(() => {
                        LoadTasks();
                        executingTasksGrid.Items.Refresh();
                    });

                    System.Console.WriteLine($"任务 '{task.TaskName}' 数据爬取完成");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"爬取失败: {ex.Message}");
                }
                finally
                {
                    _isScrapingThreadActive = false;
                }
            }

            Thread.Sleep(5000); // 每5秒检查一次队列
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Task selectedTask = pendingTasksGrid.SelectedItem as Task ?? completedTasksGrid.SelectedItem as Task;
            if (selectedTask != null)
            {
                var result = MessageBox.Show($"确定要删除任务 '{selectedTask.TaskName}' 吗？",
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DatabaseHelper.DeleteTask(selectedTask.Id);
                    LoadTasks();
                    MessageBox.Show("任务已删除！", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个任务进行删除", "未选择任务", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null && int.TryParse(button.Tag.ToString(), out int taskId))
            {
                var result = MessageBox.Show("确定要中止这个直播任务吗？", "中止确认",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // 查找任务
                    var task = DatabaseHelper.GetTaskById(taskId);
                    if (task != null)
                    {
                        // 执行中止操作并更新状态
                        DatabaseHelper.UpdateTaskStatus(taskId, "Aborted");

                        // 刷新界面
                        LoadTasks();

                        MessageBox.Show("任务已中止", "操作成功",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        public class DelayHelper
        {
            public int GetRecommendedWaitTime(string operationType)
            {
                return operationType switch
                {
                    "window_activation" => 100,    // 窗口激活
                    "button_click" => 500,         // 普通按钮点击
                    "stream_start" => 1000,        // 直播开始
                    "stream_stop" => 500,          // 直播停止
                    "platform_switch" => 3000,     // 平台切换
                    "app_launch" => 10000,         // 应用启动
                    "shutdown" => 3000,            // 关闭应用
                    "task_interval" => 2000,       // 任务间间隔
                    _ => 2000                      // 默认
                };
            }
        }
        public static void SetMaxVolume()
        {
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f; // 1.0 表示最大音量
        }
    }
