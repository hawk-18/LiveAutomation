using System.Windows;
using Microsoft.Data.Sqlite;

namespace WpfAppLiveAutomation
{
    public partial class ReplaySettingWindow : Window
    {
        public string TaskUnionId { get; }
        public string TaskName { get; }

        public ReplaySettingWindow(string taskUnionId, string taskName)
        {
            InitializeComponent();
            TaskName = taskName;
            this.Title = $"回放设置 - {taskName}";
            //TaskUnionId = DatabaseHelper.GetTaskUnionIdByTaskName(taskName) ?? "1";
            TaskUnionId = taskUnionId ;
            LoadReplayVideoInfo(TaskUnionId);
        }

        private void LoadReplayVideoInfo(string taskUnionId)
        {

            // 确保有有效的UnionId
            if (string.IsNullOrEmpty(taskUnionId) || taskUnionId == "1")
            {
                taskUnionId = DatabaseHelper.GetTaskUnionIdByTaskName(TaskName);
            }

            // 获取或创建新的回放记录
            var replayVideo = DatabaseHelper.GetLatestReplayVideoByUnionId(taskUnionId)
                            ?? new ReplayVideo { TaskUnionId = taskUnionId };

            // 安全设置所有字段
            douyinTitleTextBox.Text = replayVideo.DouyinTitle ?? "";
            douyinDescTextBox.Text = replayVideo.DouyinDescription ?? "";
            bilibiliTitleTextBox.Text = replayVideo.BilibiliTitle ?? "";
            bilibiliDescTextBox.Text = replayVideo.BilibiliDescription ?? "";
            wechatTitleTextBox.Text = replayVideo.WechatVideoTitle ?? "";
            wechatDescTextBox.Text = replayVideo.WechatVideoDescription ?? "";
            officialTitleTextBox.Text = replayVideo.OfficialAccountTitle ?? "";
            officialDescTextBox.Text = replayVideo.OfficialAccountDescription ?? "";
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var replayVideo = new ReplayVideo
            {
                TaskUnionId = TaskUnionId,
                DouyinTitle = douyinTitleTextBox.Text.Trim(),
                DouyinDescription = douyinDescTextBox.Text.Trim(),
                BilibiliTitle = bilibiliTitleTextBox.Text.Trim(),
                BilibiliDescription = bilibiliDescTextBox.Text.Trim(),
                WechatVideoTitle = wechatTitleTextBox.Text.Trim(),
                WechatVideoDescription = wechatDescTextBox.Text.Trim(),
                OfficialAccountTitle = officialTitleTextBox.Text.Trim(),
                OfficialAccountDescription = officialDescTextBox.Text.Trim()
            };

            try
            {
                //// ==== 关键修改：获取最新的UnionId ====
                //string latestUnionId = DatabaseHelper.GetTaskUnionIdByTaskName(TaskName);
                //replayVideo.TaskUnionId = latestUnionId; // 更新为最新UnionId

                //// ==== 验证UnionId存在性 ====
                //bool unionIdExists = false;
                //using (var connection = new SqliteConnection(DatabaseHelper.ConnectionString))
                //{
                //    connection.Open();
                //    var checkCmd = connection.CreateCommand();
                //    checkCmd.CommandText = "SELECT COUNT(*) FROM Tasks WHERE UnionId = $unionId AND IsDeleted = 0";
                //    checkCmd.Parameters.AddWithValue("$unionId", latestUnionId);
                //    unionIdExists = (long)checkCmd.ExecuteScalar() > 0;
                //}

                //if (!unionIdExists)
                //{
                //    MessageBox.Show($"错误：关联的任务ID '{latestUnionId}' 不存在或已被删除",
                //        "数据不一致", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                // ==== 保存回放信息（使用最新的UnionId）====
                DatabaseHelper.AddOrUpdateReplayVideo(replayVideo);

                MessageBox.Show("回放信息已保存！", "操作成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
