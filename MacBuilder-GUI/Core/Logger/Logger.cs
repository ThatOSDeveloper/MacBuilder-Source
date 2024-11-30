using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder.Core.Logger
{
    public class Logger
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug,
            Animation
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (level == LogLevel.Info && !Global.Global.EnableLogging) return;
            string logMessage = FormatLogMessage(message, level);
            ConsoleLog(logMessage, level);
            FileLog(logMessage);
        }
        private static string FormatLogMessage(string message, LogLevel level)
        {
            return $"{DateTime.Now:HH:mm:ss} ({level}): {message}";
        }

        private static void ConsoleLog(string message, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }
        private static void FileLog(string message)
        {
            string logFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
            try
            {
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
