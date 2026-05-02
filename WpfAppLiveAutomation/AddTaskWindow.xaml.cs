using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;

namespace WpfAppLiveAutomation
{
    public partial class AddTaskWindow : Window
    {
        private readonly List<string> creators = new List<string>
        {
            "陈秋颖", "周悦来", "张园园47", "关昊8", "周宇超7", "杨德志9", "孙浩凱"
        };

        public Task NewTask { get; private set; }

        public AddTaskWindow()
        {
            InitializeComponent();

            NewTask = new Task
            {
                Status = "Pending",
                CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Creator = creators[0],
                Editor = null,
                EditorTime = null,
                OriginalId = 0,
                IsDeleted = 0,
                UnionId = $"TASK_{DateTime.Now:yyyyMMddHHmmssfff}_{new Random().Next(1000, 9999)}"
            };

            DataContext = NewTask;
            SetDefaultTimes();
            SetCreatorDefault();
        }

        private void SetDefaultTimes()
        {
            DateTime now = DateTime.Now;
            DateTime prepTime = now;
            DateTime startTime = prepTime.AddMinutes(30);
            DateTime endTime = startTime.AddMinutes(30);

            startDatePicker.SelectedDate = startTime.Date;
            endDatePicker.SelectedDate = endTime.Date;
            prepDatePicker.SelectedDate = prepTime.Date;

            startTimeTextBox.Text = startTime.ToString("HH:mm:ss");
            endTimeTextBox.Text = endTime.ToString("HH:mm:ss");
            prepTimeTextBox.Text = prepTime.ToString("HH:mm:ss");

            NewTask.PreparationTime = prepTime.ToString("yyyy-MM-dd HH:mm:ss");
            NewTask.StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            NewTask.EndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void SetCreatorDefault()
        {
            string currentUser = Environment.UserName;
            int userIndex = creators.FindIndex(c => currentUser.Contains(c));

            if (userIndex >= 0)
            {
                switch (userIndex)
                {
                    case 0: rbCreator1.IsChecked = true; break;
                    case 1: rbCreator2.IsChecked = true; break;
                    case 2: rbCreator3.IsChecked = true; break;
                    case 3: rbCreator4.IsChecked = true; break;
                    case 4: rbCreator5.IsChecked = true; break;
                    case 5: rbCreator6.IsChecked = true; break;
                    case 6: rbCreator7.IsChecked = true; break;
                }
                NewTask.Creator = creators[userIndex];
            }
            else
            {
                rbCreator1.IsChecked = true;
                NewTask.Creator = creators[0];
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

            if (NewTask.Id > 0)
            {
                MessageBox.Show("任务已保存，请勿重复提交");
                return;
            }

            if (!IsValidTime(startTime) || !IsValidTime(endTime) || !IsValidTime(prepTime))
            {
                MessageBox.Show("时间格式不正确！请输入HH:mm或HH:mm:ss格式的时间",
                              "输入错误",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // 自动补全秒数
            if (startTime.Split(':').Length == 2) startTime += ":00";
            if (endTime.Split(':').Length == 2) endTime += ":00";
            if (prepTime.Split(':').Length == 2) prepTime += ":00";

            NewTask.StartTime = $"{startDatePicker.SelectedDate:yyyy-MM-dd} {startTime}";
            NewTask.EndTime = $"{endDatePicker.SelectedDate:yyyy-MM-dd} {endTime}";
            NewTask.PreparationTime = $"{prepDatePicker.SelectedDate:yyyy-MM-dd} {prepTime}";

            // 设置创建人
            if (rbCreator1.IsChecked == true) NewTask.Creator = creators[0];
            else if (rbCreator2.IsChecked == true) NewTask.Creator = creators[1];
            else if (rbCreator3.IsChecked == true) NewTask.Creator = creators[2];
            else if (rbCreator4.IsChecked == true) NewTask.Creator = creators[3];
            else if (rbCreator5.IsChecked == true) NewTask.Creator = creators[4];
            else if (rbCreator6.IsChecked == true) NewTask.Creator = creators[5];
            else if (rbCreator7.IsChecked == true) NewTask.Creator = creators[6];

            // 设置平台
            var platforms = new List<string>();
            if (cbBilibili.IsChecked == true) platforms.Add("哔哩哔哩");
            if (cbDouyin.IsChecked == true) platforms.Add("抖音");
            if (cbWeChat.IsChecked == true) platforms.Add("微信视频号");
            NewTask.Platform = string.Join(",", platforms);

            // 验证必填字段
            if (string.IsNullOrWhiteSpace(NewTask.TaskName) ||
                string.IsNullOrWhiteSpace(NewTask.LiveTitle) ||
                string.IsNullOrWhiteSpace(NewTask.VideoAddress))
            {
                MessageBox.Show("请填写所有必填字段！", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DatabaseHelper.AddTask(NewTask);
                string newUnionId = DatabaseHelper.GetTaskUnionId(NewTask.Id);

                try
                {
                    DatabaseHelper.UpdateReplayVideoUnionId("1", newUnionId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"更新UnionId失败: {ex.Message}");
                }

                NewTask.UnionId = newUnionId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存任务失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
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
                NewTask.VideoAddress = openFileDialog.FileName;
                // 显式更新绑定
                txtVideoPath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();

                // 立即计算并显示MD5
                string md5 = DatabaseHelper.CalculateFileMd5(NewTask.VideoAddress);
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
                NewTask.PosterAddress = openFileDialog.FileName;
                // 显式更新绑定
                txtPosterPath.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }


        private void ReplaySettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewTask.TaskName))
            {
                MessageBox.Show("请先填写任务名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string unionId = string.IsNullOrEmpty(NewTask.UnionId)
                ? "TEMP_" + DateTime.Now.Ticks.ToString()
                : NewTask.UnionId;

            var replayWindow = new ReplaySettingWindow(unionId, NewTask.TaskName)
            {
                Owner = this
            };
            replayWindow.ShowDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
