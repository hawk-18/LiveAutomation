using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Controls;

namespace WpfAppLiveAutomation
{
    public partial class EditTaskWindow : Window
    {
        private readonly List<string> editors = new List<string>
        {
            "陈秋颖", "周悦来", "张园园47", "关昊8", "周宇超7", "杨德志9", "孙浩凱"
        };

        private DispatcherTimer _updateTimer;

        public Task EditedTask { get; set; }

        public EditTaskWindow(Task taskToEdit)
        {
            InitializeComponent();

            EditedTask = new Task
            {
                Id = taskToEdit.Id,
                TaskName = taskToEdit.TaskName,
                StartTime = taskToEdit.StartTime,
                EndTime = taskToEdit.EndTime,
                LiveTitle = taskToEdit.LiveTitle,
                Status = taskToEdit.Status,
                Platform = taskToEdit.Platform,
                VideoAddress = taskToEdit.VideoAddress,
                PosterAddress = taskToEdit.PosterAddress,
                CreateTime = taskToEdit.CreateTime,
                Creator = taskToEdit.Creator,
                Editor = Environment.UserName,
                EditorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                PreparationTime = taskToEdit.PreparationTime,
                OriginalId = taskToEdit.OriginalId,
                IsDeleted = taskToEdit.IsDeleted,
                UnionId = taskToEdit.UnionId,
            };

            DataContext = EditedTask;
            InitializeTimeControls();
            InitializePlatformSelection();
            InitializeEditorSelection();

            txtEditTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            InitializeUpdateTimer();
        }

        private void InitializeUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateEditTime;
            _updateTimer.Start();
        }

        private void UpdateEditTime(object sender, EventArgs e)
        {
            EditedTask.EditorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtEditTime.Text = EditedTask.EditorTime;
        }

        private void InitializeTimeControls()
        {
            if (DateTime.TryParse(EditedTask.StartTime, out DateTime startTime))
            {
                startDatePicker.SelectedDate = startTime;
                startTimeTextBox.Text = startTime.ToString("HH:mm:ss");
            }

            if (DateTime.TryParse(EditedTask.EndTime, out DateTime endTime))
            {
                endDatePicker.SelectedDate = endTime;
                endTimeTextBox.Text = endTime.ToString("HH:mm:ss");
            }

            if (DateTime.TryParse(EditedTask.PreparationTime, out DateTime prepTime))
            {
                prepDatePicker.SelectedDate = prepTime;
                prepTimeTextBox.Text = prepTime.ToString("HH:mm:ss");
            }
        }

        private void InitializePlatformSelection()
        {
            if (string.IsNullOrEmpty(EditedTask.Platform)) return;

            var platforms = EditedTask.Platform.Split(',');
            foreach (var platform in platforms)
            {
                switch (platform.Trim())
                {
                    case "哔哩哔哩": cbBilibili.IsChecked = true; break;
                    case "抖音": cbDouyin.IsChecked = true; break;
                    case "微信视频号": cbWeChat.IsChecked = true; break;
                }
            }
        }

        private void InitializeEditorSelection()
        {
            string currentUser = Environment.UserName;
            int userIndex = editors.FindIndex(e => currentUser.Contains(e));

            if (userIndex >= 0)
            {
                switch (userIndex)
                {
                    case 0: rbEditor1.IsChecked = true; break;
                    case 1: rbEditor2.IsChecked = true; break;
                    case 2: rbEditor3.IsChecked = true; break;
                    case 3: rbEditor4.IsChecked = true; break;
                    case 4: rbEditor5.IsChecked = true; break;
                    case 5: rbEditor6.IsChecked = true; break;
                    case 6: rbEditor7.IsChecked = true; break;
                }
                EditedTask.Editor = editors[userIndex];
            }
            else
            {
                rbEditor1.IsChecked = true;
                EditedTask.Editor = editors[0];
            }
        }

        private void BrowseVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择视频文件",
                Filter = "视频文件 (*.mp4;*.mov;*.avi;*.mkv)|*.mp4;*.mov;*.avi;*.mkv|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                EditedTask.VideoAddress = openFileDialog.FileName;
                // 显式更新绑定
                txtVideoPath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private void BrowsePosterButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择封面图片",
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                EditedTask.PosterAddress = openFileDialog.FileName;
                // 显式更新绑定
                txtPosterPath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }


        private bool IsValidTime(string time)
        {
            return Regex.IsMatch(time, @"^([01]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string startTime = startTimeTextBox?.Text?.Trim() ?? "";
            string endTime = endTimeTextBox?.Text?.Trim() ?? "";
            string prepTime = prepTimeTextBox?.Text?.Trim() ?? "";

            if (!IsValidTime(startTime) || !IsValidTime(endTime) || !IsValidTime(prepTime))
            {
                MessageBox.Show("时间格式不正确！请输入HH:mm或HH:mm:ss格式的时间",
                              "输入错误",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (startTime.Split(':').Length == 2) startTime += ":00";
            if (endTime.Split(':').Length == 2) endTime += ":00";
            if (prepTime.Split(':').Length == 2) prepTime += ":00";

            EditedTask.StartTime = $"{startDatePicker.SelectedDate:yyyy-MM-dd} {startTime}";
            EditedTask.EndTime = $"{endDatePicker.SelectedDate:yyyy-MM-dd} {endTime}";
            EditedTask.PreparationTime = $"{prepDatePicker.SelectedDate:yyyy-MM-dd} {prepTime}";
            EditedTask.EditorTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtEditTime.Text = EditedTask.EditorTime;

            if (rbEditor1.IsChecked == true) EditedTask.Editor = editors[0];
            else if (rbEditor2.IsChecked == true) EditedTask.Editor = editors[1];
            else if (rbEditor3.IsChecked == true) EditedTask.Editor = editors[2];
            else if (rbEditor4.IsChecked == true) EditedTask.Editor = editors[3];
            else if (rbEditor5.IsChecked == true) EditedTask.Editor = editors[4];
            else if (rbEditor6.IsChecked == true) EditedTask.Editor = editors[5];
            else if (rbEditor7.IsChecked == true) EditedTask.Editor = editors[6];

            var platforms = new List<string>();
            if (cbBilibili.IsChecked == true) platforms.Add("哔哩哔哩");
            if (cbDouyin.IsChecked == true) platforms.Add("抖音");
            if (cbWeChat.IsChecked == true) platforms.Add("微信视频号");
            EditedTask.Platform = string.Join(",", platforms);

            if (string.IsNullOrWhiteSpace(EditedTask.TaskName) ||
                string.IsNullOrWhiteSpace(EditedTask.LiveTitle) ||
                string.IsNullOrWhiteSpace(EditedTask.VideoAddress))
            {
                MessageBox.Show("请填写所有必填字段！", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DatabaseHelper.UpdateTask(EditedTask);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新任务失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void ReplaySettingButton_Click(object sender, RoutedEventArgs e)
        {
            string latestUnionId = DatabaseHelper.GetTaskUnionId(EditedTask.Id) ?? EditedTask.UnionId;

            var replayWindow = new ReplaySettingWindow(latestUnionId, EditedTask.TaskName)
            {
                Owner = this
            };

            replayWindow.Show();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Tick -= UpdateEditTime;
            }
        }
    }
}
