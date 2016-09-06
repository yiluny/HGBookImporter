using log4net;
using System;

namespace HG.Coprorate.Firebrand.CustomLogs
{
    public class BooksImportLog
    {
        private static string bookImportLogName = "Sitecore.Diagnostics.BookImport";

        private static readonly ILog _logger = log4net.LogManager.GetLogger(bookImportLogName);

        public static void Info(string message)
        {
            _logger.Info(message);
        }

        public static void Info(string message, Exception exception)
        {
            _logger.Info(message, exception);
        }

        public static void Warn(string message)
        {
            _logger.Warn(message);
        }

        public static void Warn(string message, Exception exception)
        {
            _logger.Warn(message, exception);
        }

        public static void Error(string message)
        {
            _logger.Error(message);
        }

        public static void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }
    }
}