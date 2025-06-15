using System.Diagnostics;

namespace WowWtfSync.WindowsApp
{
    public static class Logger
    {
        /**
         * This static class is responsible for logging messages to a log.txt file.
         */

        private static readonly string logFilePath = Path.Combine(
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) ?? string.Empty,
            "log.txt"
        );

        /**
         * This method logs a message to the log.txt file with the specified log type. A keyword
         * can also be provided to filter or categorize the log entry.
         */
        public static void Log(string message, LoggerLogTypes logType, string messageKeyword="")
        {
            string logTypeString = Logger.GetLogTypeString(logType);
            string messageKeywordString = string.IsNullOrEmpty(messageKeyword)
                ? string.Empty
                : $" (keyword={messageKeyword}) ";
            try
            {
                File.AppendAllText(
                    logFilePath,
                    $"[{logTypeString}]{messageKeywordString} {DateTime.Now}: {message}{Environment.NewLine}"
                );
            }
            catch (Exception ex)
            {
                // Handle exceptions related to logging, e.g., file access issues
                MessageBox.Show(
                    $"Failed to write to log.txt: {ex.Message}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        public static List<string> GetLogMessages(string messageKeyword="")
        {
            List<string> logMessages = new List<string>();
            try
            {
                if (File.Exists(logFilePath))
                {
                    logMessages = File.ReadAllLines(logFilePath).ToList();
                    logMessages = logMessages
                        .Where(line => !string.IsNullOrWhiteSpace(line)) // Filter out empty lines
                        .Select(line => line.Trim()) // Trim whitespace from each line
                        .ToList();
                    if (!string.IsNullOrEmpty(messageKeyword))
                    {
                        // Filter log messages by keyword if provided
                        logMessages = logMessages
                            .Where(line => line.Contains(
                                $"(keyword={messageKeyword})",
                                StringComparison.OrdinalIgnoreCase
                            ))
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions related to reading the log file
                MessageBox.Show(
                    $"Failed to read log.txt: {ex.Message}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            return logMessages;
        }

        /**
         * This method returns a string representation of the log type.
         */
        private static string GetLogTypeString(LoggerLogTypes logType)
        {
            return logType switch
            {
                LoggerLogTypes.Info => "INFO",
                LoggerLogTypes.Warning => "WARNING",
                LoggerLogTypes.Error => "ERROR",
                _ => "UNKNOWN"
            };
        }
    }
}
