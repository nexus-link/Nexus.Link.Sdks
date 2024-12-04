using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Serilog;
using Serilog.Events;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class SerilogLogger(ILogger logger) : ISyncLogger, IFallbackLogger, IAsyncLogger
    {
        /// <inheritdoc />
        public void SafeLog(LogSeverityLevel logSeverityLevel, string message)
        {
            PrivateSafeLog(logSeverityLevel, message, null);
        }

        private void PrivateSafeLog(LogSeverityLevel logSeverityLevel, string message, Exception exception = null)
        {
            try
            {
                LogEventLevel logLevel;
                switch (logSeverityLevel)
                {
                    case LogSeverityLevel.Verbose:
                        logLevel = LogEventLevel.Verbose;
                        break;
                    case LogSeverityLevel.Information:
                        logLevel = LogEventLevel.Information;
                        break;
                    case LogSeverityLevel.Warning:
                        logLevel = LogEventLevel.Warning;
                        break;
                    case LogSeverityLevel.Error:
                        logLevel = LogEventLevel.Error;
                        break;
                    case LogSeverityLevel.Critical:
                        logLevel = LogEventLevel.Fatal;
                        break;
                    // ReSharper disable once RedundantCaseLabel
                    case LogSeverityLevel.None:
                        return;
                    default:
                        logger.Write(LogEventLevel.Fatal, $"Unexpected {nameof(logSeverityLevel)} ({logSeverityLevel}) for message:\r{message}.");
                        return;
                }
                logger.Write(logLevel, exception, message);
            }
            catch (Exception)
            {
                // This method should never fail.
            }
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            PrivateSafeLog(logRecord.SeverityLevel, ToLogString(logRecord), logRecord.Exception);
        }

        /// <inheritdoc />
        public Task LogAsync(LogRecord logRecord, CancellationToken cancellationToken = default)
        {
            LogSync(logRecord);
            return Task.CompletedTask;
        }

        private string ToLogString(LogRecord logRecord)
        {
            var correlationId = FulcrumApplication.Context.CorrelationId;
            var stringBuilder = new StringBuilder(logRecord.TimeStamp.ToLogString());
            if (!string.IsNullOrWhiteSpace(correlationId)) stringBuilder.Append($" {correlationId}");
            stringBuilder.Append(" ");
            stringBuilder.Append(logRecord.SeverityLevel.ToString());
            var context = ContextToLogString();
            if (!string.IsNullOrWhiteSpace(context))
            {
                stringBuilder.Append(" ");
                stringBuilder.AppendLine(context);
            }
            else
            {
                stringBuilder.AppendLine("");
            }
            stringBuilder.Append(logRecord.Message);
            return stringBuilder.ToString();
        }

        private string ContextToLogString()
        {
            var stringBuilder = new StringBuilder($"{FulcrumApplication.Setup.Tenant} {FulcrumApplication.Setup.Name} ({FulcrumApplication.Setup.RunTimeLevel}) context:{ FulcrumApplication.Context.ContextId}");

            var clientTenant = FulcrumApplication.Context.ClientTenant;
            if (clientTenant != null)
            {
                stringBuilder.Append(" tenant: ");
                stringBuilder.Append(clientTenant.ToLogString());
            }

            var clientName = FulcrumApplication.Context.CallingClientName;
            if (!string.IsNullOrWhiteSpace(clientName))
            {
                stringBuilder.Append(" client: ");
                stringBuilder.Append(clientName);
            }
            return stringBuilder.ToString();
        }
    }
}

