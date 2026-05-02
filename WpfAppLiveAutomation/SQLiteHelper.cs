using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

public static class DatabaseHelper
{
    private static readonly string DbPath = Path.Combine(
        @"D:\LiveTaskManager",
        "LiveTaskManager.db"
    );

    public static string ConnectionString => $"Data Source={DbPath}";

    public static bool TestConnection()
    {
        try
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public static void InitializeDatabase()
    {
        var dir = Path.GetDirectoryName(DbPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();

            // 启用外键支持
            var enableForeignKeys = connection.CreateCommand();
            enableForeignKeys.CommandText = "PRAGMA foreign_keys = ON";
            enableForeignKeys.ExecuteNonQuery();

            SqliteCommand createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Tasks (
                id            INTEGER PRIMARY KEY AUTOINCREMENT,
                UnionId       TEXT NOT NULL, 
                TaskName      TEXT NOT NULL,
                StartTime     TEXT NOT NULL,
                EndTime       TEXT NOT NULL,
                LiveTitle     TEXT NOT NULL,
                Status        TEXT NOT NULL,
                Platform      TEXT NOT NULL,
                CreateTime    TEXT NOT NULL,
                Creator       TEXT NOT NULL,
                VideoAddress  TEXT NOT NULL,
                PosterAddress TEXT,
                Editor TEXT,
                EditorTime TEXT,
                PreparationTime TEXT NOT NULL,
                OriginalId INTEGER DEFAULT 0,
                IsDeleted INTEGER DEFAULT 0,
                VideoMD5 TEXT
            )";
            createTableCommand.ExecuteNonQuery();


            // 创建新的 ReplayVideos 表
            SqliteCommand createReplayTableCommand = connection.CreateCommand();
            createReplayTableCommand.CommandText =
            @"
        CREATE TABLE IF NOT EXISTS ReplayVideos (
            id                      INTEGER PRIMARY KEY AUTOINCREMENT,
            TaskUnionId             TEXT NOT NULL,  -- 使用 TEXT 类型匹配 Tasks.UnionId
            DouyinTitle             TEXT,
            DouyinDescription       TEXT,
            BilibiliTitle           TEXT,
            BilibiliDescription     TEXT,
            WechatVideoTitle        TEXT,
            WechatVideoDescription  TEXT,
            OfficialAccountTitle    TEXT,
            OfficialAccountDescription TEXT
        )";
            createReplayTableCommand.ExecuteNonQuery();

            // 创建索引
            SqliteCommand createIndexCommand = connection.CreateCommand();
            createIndexCommand.CommandText =
            "CREATE INDEX IF NOT EXISTS idx_replayvideos_taskunionid ON ReplayVideos (TaskUnionId)";
            createIndexCommand.ExecuteNonQuery();
        }
    }




    public static void UpdateVideoMd5(string videoAddress, string md5Value)
    {
        if (string.IsNullOrEmpty(videoAddress)) return;

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tasks SET VideoMD5 = $md5 WHERE VideoAddress = $address";
            command.Parameters.AddWithValue("$md5", md5Value ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$address", videoAddress);
            command.ExecuteNonQuery();
        }
    }

    public static string GetVideoMd5(string videoAddress)
    {
        if (string.IsNullOrEmpty(videoAddress)) return null;

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT VideoMD5 FROM Tasks WHERE VideoAddress = $address";
            command.Parameters.AddWithValue("$address", videoAddress);

            var result = command.ExecuteScalar();
            return result == DBNull.Value || result == null ? null : (string)result;
        }
    }

    public static List<Task> GetAllTasks()
    {
        var tasks = new List<Task>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tasks WHERE IsDeleted = 0";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tasks.Add(new Task
                    {
                        Id = reader.GetInt32(0),
                        UnionId = reader.GetString(1), // 新增字段
                        TaskName = reader.GetString(2),
                        StartTime = reader.GetString(3),
                        EndTime = reader.GetString(4),
                        LiveTitle = reader.GetString(5),
                        Status = reader.GetString(6),
                        Platform = reader.GetString(7),
                        CreateTime = reader.GetString(8),
                        Creator = reader.GetString(9),
                        VideoAddress = reader.GetString(10),
                        PosterAddress = reader.IsDBNull(11) ? null : reader.GetString(11),
                        Editor = reader.IsDBNull(12) ? null : reader.GetString(12),
                        EditorTime = reader.IsDBNull(13) ? null : reader.GetString(13),
                        PreparationTime = reader.GetString(14),
                        OriginalId = reader.GetInt32(15),
                        IsDeleted = reader.GetInt32(16),
                        VideoMD5 = reader.IsDBNull(17) ? null : reader.GetString(17)  // MD5字段
                    });
                }
            }
        }
        return tasks;
    }

    public static void AddTask(Task task)
    {
        task.StartTime = FormatDateTime(task.StartTime);
        task.EndTime = FormatDateTime(task.EndTime);
        task.PreparationTime = FormatDateTime(task.PreparationTime);
        task.CreateTime = FormatDateTime(task.CreateTime);
        if (task.UnionId == "1")

        {
            task.UnionId = $"TASK_{DateTime.Now:yyyyMMddHHmmssfff}_{new Random().Next(1000, 9999)}";
        }
        // 自动计算视频文件的MD5
        if (!string.IsNullOrEmpty(task.VideoAddress) && File.Exists(task.VideoAddress))
        {
            task.VideoMD5 = CalculateFileMd5(task.VideoAddress);
        }
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            command.CommandText = @"
            INSERT INTO Tasks (
                UnionId, TaskName, StartTime, EndTime, LiveTitle,
                Status, Platform, CreateTime, Creator, VideoAddress,
                PosterAddress, Editor, EditorTime, PreparationTime,
                OriginalId, IsDeleted, VideoMD5
            )
            VALUES (
                $unionId, $taskName, $startTime, $endTime, $liveTitle,
                $status, $platform, $createTime, $creator, $videoAddress,
                $posterAddress, $editor, $editorTime, $preparationTime,
                $originalId, $isDeleted, $videoMD5
            )";
            command.Parameters.AddWithValue("$unionId", task.UnionId);
            command.Parameters.AddWithValue("$taskName", task.TaskName);
            command.Parameters.AddWithValue("$startTime", task.StartTime);
            command.Parameters.AddWithValue("$endTime", task.EndTime);
            command.Parameters.AddWithValue("$liveTitle", task.LiveTitle);
            command.Parameters.AddWithValue("$status", task.Status);
            command.Parameters.AddWithValue("$platform", task.Platform);
            command.Parameters.AddWithValue("$createTime", task.CreateTime);
            command.Parameters.AddWithValue("$creator", task.Creator);
            command.Parameters.AddWithValue("$videoAddress", task.VideoAddress);
            command.Parameters.AddWithValue("$posterAddress", task.PosterAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$editor", task.Editor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$editorTime", task.EditorTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$preparationTime", task.PreparationTime);
            command.Parameters.AddWithValue("$originalId", task.OriginalId);
            command.Parameters.AddWithValue("$isDeleted", task.IsDeleted);
            command.Parameters.AddWithValue("$videoMD5", task.VideoMD5 ?? (object)DBNull.Value);  // MD5参数
            
            command.ExecuteNonQuery();
        }
    }

    public static void UpdateTask(Task task)
    {
        task.StartTime = FormatDateTime(task.StartTime);
        task.EndTime = FormatDateTime(task.EndTime);
        task.PreparationTime = FormatDateTime(task.PreparationTime);
        task.EditorTime = FormatDateTime(task.EditorTime);

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();

            // 完整包含VideoMD5字段
            command.CommandText =
            @"
            UPDATE Tasks
            SET TaskName = $taskName,
                StartTime = $startTime,
                EndTime = $endTime,
                LiveTitle = $liveTitle,
                Status = $status,
                Platform = $platform,
                VideoAddress = $videoAddress,
                PosterAddress = $posterAddress,
                Editor = $editor,
                EditorTime = $editorTime,
                PreparationTime = $preparationTime,
                OriginalId = $originalId,
                IsDeleted = $isDeleted,
                VideoMD5 = $videoMD5
            WHERE Id = $id
            ";

            command.Parameters.AddWithValue("$id", task.Id);
            command.Parameters.AddWithValue("$taskName", task.TaskName);
            command.Parameters.AddWithValue("$startTime", task.StartTime);
            command.Parameters.AddWithValue("$endTime", task.EndTime);
            command.Parameters.AddWithValue("$liveTitle", task.LiveTitle);
            command.Parameters.AddWithValue("$status", task.Status);
            command.Parameters.AddWithValue("$platform", task.Platform);
            command.Parameters.AddWithValue("$videoAddress", task.VideoAddress);
            command.Parameters.AddWithValue("$posterAddress", task.PosterAddress ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$editor", task.Editor ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$editorTime", task.EditorTime ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$preparationTime", task.PreparationTime);
            command.Parameters.AddWithValue("$originalId", task.OriginalId);
            command.Parameters.AddWithValue("$isDeleted", task.IsDeleted);
            command.Parameters.AddWithValue("$videoMD5", task.VideoMD5 ?? (object)DBNull.Value);  // MD5参数

            command.ExecuteNonQuery();
        }
    }

    public static void UpdateTaskStatus(int taskId, string newStatus)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Tasks SET Status = $status WHERE id = $id";
            command.Parameters.AddWithValue("$status", newStatus);
            command.Parameters.AddWithValue("$id", taskId);
            command.ExecuteNonQuery();
        }
    }

    public static Task GetTaskById(int taskId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Tasks WHERE id = $id";
            command.Parameters.AddWithValue("$id", taskId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Task
                    {
                        Id = reader.GetInt32(0),
                        UnionId = reader.GetString(1),  // 新增字段
                        TaskName = reader.GetString(2),
                        StartTime = reader.GetString(3),
                        EndTime = reader.GetString(4),
                        LiveTitle = reader.GetString(5),
                        Status = reader.GetString(6),
                        Platform = reader.GetString(7),
                        CreateTime = reader.GetString(8),
                        Creator = reader.GetString(9),
                        VideoAddress = reader.GetString(10),
                        PosterAddress = reader.IsDBNull(11) ? null : reader.GetString(11),
                        Editor = reader.IsDBNull(12) ? null : reader.GetString(12),
                        EditorTime = reader.IsDBNull(13) ? null : reader.GetString(13),
                        PreparationTime = reader.IsDBNull(14) ? null : reader.GetString(14), // 修复这里
                        OriginalId = reader.GetInt32(15),
                        IsDeleted = reader.GetInt32(16),
                        VideoMD5 = reader.IsDBNull(17) ? null : reader.GetString(17)
                    };
                }
            }
        }
        return null;
    }


    public static void DeleteTask(int taskId)
    {
        try
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Tasks SET IsDeleted = 1 WHERE id = $id";
                command.Parameters.AddWithValue("$id", taskId);
                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除任务失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public static void AddOrUpdateReplayVideo(ReplayVideo replayVideo)
    {
        if (replayVideo == null)
            throw new ArgumentNullException(nameof(replayVideo));

        // 确保 UnionId 不为空
        if (string.IsNullOrEmpty(replayVideo.TaskUnionId))
        {
            replayVideo.TaskUnionId = "1";
        }

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();

            // 启用外键支持
            var enableCmd = connection.CreateCommand();
            enableCmd.CommandText = "PRAGMA foreign_keys = ON;";
            enableCmd.ExecuteNonQuery();

            var command = connection.CreateCommand();

            // ==== 使用 INSERT OR REPLACE 简化操作 ====
            command.CommandText = @"
            INSERT OR REPLACE INTO ReplayVideos (
                TaskUnionId, 
                DouyinTitle, 
                DouyinDescription, 
                BilibiliTitle, 
                BilibiliDescription, 
                WechatVideoTitle, 
                WechatVideoDescription, 
                OfficialAccountTitle, 
                OfficialAccountDescription
            )
            VALUES (
                $taskUnionId, 
                $douyinTitle, 
                $douyinDesc, 
                $bilibiliTitle,
                $bilibiliDesc, 
                $wechatTitle, 
                $wechatDesc,
                $officialTitle, 
                $officialDesc
            )";
            command.Parameters.AddWithValue("$taskUnionId", replayVideo.TaskUnionId);
            command.Parameters.AddWithValue("$douyinTitle", replayVideo.DouyinTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$douyinDesc", replayVideo.DouyinDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$bilibiliTitle", replayVideo.BilibiliTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$bilibiliDesc", replayVideo.BilibiliDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$wechatTitle", replayVideo.WechatVideoTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$wechatDesc", replayVideo.WechatVideoDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$officialTitle", replayVideo.OfficialAccountTitle ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$officialDesc", replayVideo.OfficialAccountDescription ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }
    }

    public static ReplayVideo GetReplayVideoByUnionId(string unionId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ReplayVideos WHERE TaskUnionId = $unionId";
            command.Parameters.AddWithValue("$unionId", unionId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new ReplayVideo
                    {
                        Id = reader.GetInt32(0),
                        TaskUnionId = reader.GetString(1),
                        DouyinTitle = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DouyinDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                        BilibiliTitle = reader.IsDBNull(4) ? null : reader.GetString(4),
                        BilibiliDescription = reader.IsDBNull(5) ? null : reader.GetString(5),
                        WechatVideoTitle = reader.IsDBNull(6) ? null : reader.GetString(6),
                        WechatVideoDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                        OfficialAccountTitle = reader.IsDBNull(8) ? null : reader.GetString(8),
                        OfficialAccountDescription = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }
            }
        }
        return null;
    }
    // 修改方法签名，使用 string 类型的 unionId
    public static void UpdateReplayVideoUnionId(string oldUnionId, string newUnionId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var enableForeignKeys = connection.CreateCommand();
            enableForeignKeys.CommandText = "PRAGMA foreign_keys = ON";
            enableForeignKeys.ExecuteNonQuery();

            // 检查新UnionId是否存在
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Tasks WHERE UnionId = $newUnionId";
            checkCmd.Parameters.AddWithValue("$newUnionId", newUnionId); // 添加这行


            var command = connection.CreateCommand();
            command.CommandText = @"
            UPDATE ReplayVideos 
            SET TaskUnionId = $newUnionId 
            WHERE TaskUnionId = $oldUnionId";

            command.Parameters.AddWithValue("$oldUnionId", oldUnionId);
            command.Parameters.AddWithValue("$newUnionId", newUnionId);

            command.ExecuteNonQuery();
        }
    }




    private static string FormatDateTime(string dateTime)
    {
        if (string.IsNullOrEmpty(dateTime)) return dateTime;

        if (DateTime.TryParse(dateTime, out DateTime dt))
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        return dateTime;
    }
    // 在DatabaseHelper中添加
    public static string GetTaskUnionId(int taskId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.Parameters.AddWithValue("$id", taskId);
            command.CommandText = "SELECT UnionId FROM Tasks WHERE id = $id";
            return command.ExecuteScalar()?.ToString();
        }
    }
    // 新增方法：通过任务名称获取最新的UnionId
    public static string GetTaskUnionIdByTaskName(string taskName)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT UnionId FROM Tasks WHERE TaskName = $name AND IsDeleted = 0 ORDER BY id DESC LIMIT 1";
            command.Parameters.AddWithValue("$name", taskName);
            return command.ExecuteScalar()?.ToString() ?? "1";
        }
    }
    // 新增方法：获取指定UnionId的最新回放记录（主键最大的记录）
    // 获取指定UnionId的最新回放记录（主键最大的记录）
    public static ReplayVideo GetLatestReplayVideoByUnionId(string unionId)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT * 
            FROM ReplayVideos 
            WHERE TaskUnionId = $unionId
            ORDER BY id DESC
            LIMIT 1";

            command.Parameters.AddWithValue("$unionId", unionId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new ReplayVideo
                    {
                        Id = reader.GetInt32(0),
                        TaskUnionId = reader.GetString(1),
                        DouyinTitle = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DouyinDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                        BilibiliTitle = reader.IsDBNull(4) ? null : reader.GetString(4),
                        BilibiliDescription = reader.IsDBNull(5) ? null : reader.GetString(5),
                        WechatVideoTitle = reader.IsDBNull(6) ? null : reader.GetString(6),
                        WechatVideoDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                        OfficialAccountTitle = reader.IsDBNull(8) ? null : reader.GetString(8),
                        OfficialAccountDescription = reader.IsDBNull(9) ? null : reader.GetString(9)
                    };
                }
            }
        }
        return null;
    }
    public static string CalculateFileMd5(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
    public static bool IsVideoMd5Unique(string videoMd5)
    {
        if (string.IsNullOrEmpty(videoMd5)) return false;

        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Tasks WHERE VideoMD5 = $md5 AND IsDeleted = 0";
            command.Parameters.AddWithValue("$md5", videoMd5);

            var count = (long)command.ExecuteScalar();
            return count == 1; // 只有当前任务使用这个MD5才返回true
        }
    }


        public static void SaveScrapedData(int taskId, string platform, object data)
        {
            using (var connection = new SqliteConnection("Data Source=live_automation.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO scraped_data (task_id, platform, data, scraped_time)
                VALUES ($taskId, $platform, $data, datetime('now'))
            ";

                command.Parameters.AddWithValue("$taskId", taskId);
                command.Parameters.AddWithValue("$platform", platform);
                command.Parameters.AddWithValue("$data", JsonConvert.SerializeObject(data));

                command.ExecuteNonQuery();
            }
        }

}

public class Task
{
    public int Id { get; set; }
    public string TaskName { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string LiveTitle { get; set; }
    public string Status { get; set; }
    public string Platform { get; set; }
    public string CreateTime { get; set; }
    public string Creator { get; set; }
    public string VideoAddress { get; set; }
    public string PosterAddress { get; set; }
    public string Editor { get; set; }
    public string EditorTime { get; set; }
    public string PreparationTime { get; set; }
    public int OriginalId { get; set; }
    public int IsDeleted { get; set; }
    public string VideoMD5 { get; set; }  // MD5属性
    // 新增UnionId字段
    public string UnionId { get; set; } // 新增字段
}
public class ReplayVideo
{
    public int Id { get; set; }
    // 强制默认值为 "1"
    public string TaskUnionId { get; set; } = "1";
    public string DouyinTitle { get; set; }
    public string DouyinDescription { get; set; }
    public string BilibiliTitle { get; set; }
    public string BilibiliDescription { get; set; }
    public string WechatVideoTitle { get; set; }
    public string WechatVideoDescription { get; set; }
    public string OfficialAccountTitle { get; set; }
    public string OfficialAccountDescription { get; set; }
}
