using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class VideoMd5Helper
{
    // 计算视频文件的 MD5 值
    public static string CalculateVideoMd5(string videoPath)
    {
        // 1. 验证文件是否存在
        if (!File.Exists(videoPath))
        {
            throw new FileNotFoundException("视频文件不存在", videoPath);
        }

        // 2. 使用 MD5 算法计算哈希
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(videoPath))
        {
            byte[] hashBytes = md5.ComputeHash(stream);

            // 3. 将字节数组转换为十六进制字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // 格式化为两位十六进制
            }

            return sb.ToString().ToLower(); // 返回小写格式的MD5
        }
    }

    // 存储到数据库（使用之前设计的 DatabaseHelper）
    public static void StoreMd5ToDatabase(string videoPath, string md5Value)
    {
        DatabaseHelper.UpdateVideoMd5(videoPath, md5Value);
    }

    // 完整流程：计算并存储
    public static string ProcessVideoMd5(string videoPath)
    {
        string md5 = CalculateVideoMd5(videoPath);
        StoreMd5ToDatabase(videoPath, md5);
        return md5;
    }
}
