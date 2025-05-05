using System;
using System.IO;

namespace FasterPrefs.Tests
{
    public static class TestHelper
    {
        public static string GetTemporaryFilePath()
        {
            string tempPath = Path.GetTempPath();
            string fileName = $"FasterPrefs_Test_{Guid.NewGuid()}.dat";
            return Path.Combine(tempPath, fileName);
        }

        public static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
