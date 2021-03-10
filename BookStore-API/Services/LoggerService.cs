using BookStore_API.Contracts;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class LoggerService : ILoggerService
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public void LogError(string message)
        {
            logger.Error(message);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogWarn(string message)
        {
            logger.Warn(message);
        }

        public void Log(string message, Exception exception)
        {
            string logMessage;
            if (exception != null)
                logMessage = $"{message}\n{ErrorMessage(exception)}";
            else
                logMessage = message;

            LogError(logMessage);
        }

        private string ErrorMessage(Exception ex)
        {
            StringBuilder mensaje = new StringBuilder();

            while (ex != null)
            {
                mensaje.AppendLine("----------");
                mensaje.AppendLine(string.Format("Source: {0}", ex.Source));
                mensaje.AppendLine(string.Format("Message: {0}", ex.Message));
                mensaje.AppendLine(string.Format("Stack Trace: {0}", ex.StackTrace));
                mensaje.AppendLine("----------");

                ex = ex.InnerException;
            }


            return mensaje.ToString();
        }

    }
}
