using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.FileManager
{
    public class FileManagerClass
    {
        public static void DeleteAllFilesInDirectory(string directory)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not delete all files in directory: {directory} {ex.Message}", Logger.LogLevel.Error);
            }
        }
        public static void DeleteFile(string FilePath) 
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not delete file: {FilePath} {ex.Message}", Logger.LogLevel.Error);
            }
        }
        public static void ExtractZipFile(string zipFilePath)
        {
            string extractPath = Path.Combine(Global.DownloadPath, "MacBuilder");

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            Directory.CreateDirectory(extractPath);

            Logger.Log($"Extracting ZIP file to {extractPath}", Logger.LogLevel.Info);
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            Logger.Log("Extraction complete.", Logger.LogLevel.Info);
        }
        public static void CreateFile(string FilePath)
        {
            try
            {
                File.Create(FilePath);
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not create file: {FilePath} {ex.Message}", Logger.LogLevel.Error);
            }
        }
        public static void CreateDirectory(string DirectoryPath)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            catch (Exception ex)
            {
                Logger.Log($"Could not create directory: {DirectoryPath} {ex.Message}", Logger.LogLevel.Error);
            }
        }
        public static void MoveFile(string FilePath, string NewFilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string destinationDir = Path.GetDirectoryName(NewFilePath);

                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    File.Move(FilePath, NewFilePath);
                }
                else
                {
                    Logger.Log($"File not found: {FilePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Unexpected error while moving file from {FilePath} to {NewFilePath}: {ex.Message}");
            }
        }
    }
}
