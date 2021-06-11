using System;
using System.Threading;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Logger.Sdk.Helpers;
using Nexus.Link.Logger.Sdk.RestClients;
using System.Threading.Tasks;

namespace Nexus.Link.Logger.Sdk
{
    /// <summary>
    /// A logger that implements IAsyncLogger
    /// </summary>
    public class FulcrumLogger : IAsyncLogger, ISyncLogger
    {
        private readonly ILogClient _legacyLoggerClient;
        private readonly ISyncLogger _messagesBuffer;
        private readonly ILogQueueHelper<LogMessage> _storageHandler;

        /// <param name="loggingServiceConfiguration">Access to TenantLoggingConfiguration</param>
        public FulcrumLogger(ILeverServiceConfiguration loggingServiceConfiguration) : this(new LogQueueHelper<LogMessage>(loggingServiceConfiguration))
        {
        }

        /// <param name="baseUri">Where the fundamentals log service is located.</param>
        /// <param name="authenticationCredentials">Credentials to the log service.</param>
        [Obsolete("Using fundamentals' logging endpoint is discouraged")]
        public FulcrumLogger(string baseUri, ServiceClientCredentials authenticationCredentials) :
             this(new LogClient(baseUri, authenticationCredentials), new LogQueueHelper<LogMessage>())
        {
        }

        /// <param name="logClient">The fundamentals log client</param>
        [Obsolete("Using fundamentals' logging endpoint is discouraged")]
        public FulcrumLogger(ILogClient logClient) :
            this(logClient, new LogQueueHelper<LogMessage>())
        {
        }

        /// <param name="logClient">The fundamentals log client</param>
        /// <param name="loggingServiceConfiguration">Access to TenantLoggingConfiguration</param>
        [Obsolete("Using fundamentals' logging endpoint is discouraged")]
        public FulcrumLogger(ILogClient logClient, ILeverServiceConfiguration loggingServiceConfiguration) :
            this(logClient, new LogQueueHelper<LogMessage>(loggingServiceConfiguration))
        {
        }

        /// <summary>
        /// Unit Tests Constructor
        /// </summary>
        public FulcrumLogger(ILogQueueHelper<LogMessage> logQueueHelper)
        {
            InternalContract.RequireNotNull(logQueueHelper, nameof(logQueueHelper));

            _messagesBuffer = new QueueToAsyncLogger(this);
            _storageHandler = logQueueHelper;
        }

        /// <summary>
        /// Legacy Unit Tests Constructor
        /// </summary>
        [Obsolete("Using fundamentals' logging endpoint is discouraged")]
        public FulcrumLogger(ILogClient logClient, ILogQueueHelper<LogMessage> logQueueHelper) : this(logQueueHelper)
        {
            _legacyLoggerClient = logClient;
        }

        /// <summary>
        /// Not implemented, use <see cref="LogAsync"/> instead.
        /// </summary>
        public void Log(LogSeverityLevel logSeverityLevel, string message)
        {
            throw new FulcrumNotImplementedException("Obsolete");
        }

        /// <inheritdoc />
        public async Task LogAsync(LogRecord logRecord, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(logRecord, nameof(logRecord));
            InternalContract.RequireValidated(logRecord, nameof(logRecord));
            var tenant = FulcrumApplication.Context.ClientTenant ?? FulcrumApplication.Setup.Tenant;
            FulcrumAssert.IsNotNull(tenant, null, "We have neither ClientTenant nor ApplicationTenant");
            var exceptionMessage = logRecord.Exception == null
                ? ""
                : $" {logRecord.Exception.ToLogString()}";
            var logMessage = new LogMessage
            {
                Originator = FulcrumApplication.Setup.Name,
                CallingClientName = FulcrumApplication.Context.CallingClientName,
                CorrelationId = FulcrumApplication.Context.CorrelationId,
                Organization = tenant?.Organization,
                Environment = tenant?.Environment,
                SeverityLevel = logRecord.SeverityLevel,
                Timestamp = logRecord.TimeStamp,
                Location = logRecord.Location,
                Message = $"{logRecord.Message}{exceptionMessage}"
            };
            FulcrumAssert.IsValidated(logMessage,
                typeof(FulcrumLogger).Namespace + ": 96F4F38C-4A06-4DF9-A7F0-105134EA30C5");

            // Exceptions are handled in QueueToAsyncLogger by LogHelper.FallbackToSimpleLoggingFailSafe
            var (hasStorageQueue, writableQueue) = await _storageHandler.TryGetQueueAsync(tenant, cancellationToken);
            if (hasStorageQueue)
            {
                // Log to Azure Storage Queue for tenant
                await writableQueue.AddMessageAsync(logMessage, cancellationToken: cancellationToken);
            }
            else
            {
                FulcrumAssert.IsNotNull(_legacyLoggerClient, null, "When using this logger without a storage queue configuration, an ILogClient must be provided.");

                // Fallback REST client logger to fundamentals
                await _legacyLoggerClient.LogAsync(tenant, cancellationToken, logMessage);
            }
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            _messagesBuffer.LogSync(logRecord);
        }
    }
}