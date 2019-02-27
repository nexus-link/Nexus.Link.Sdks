using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Logger.Sdk.RestClients;

namespace Nexus.Link.Logger.Sdk
{
    /// <summary>
    /// A logger that implements IAsyncLogger
    /// </summary>
    public class FulcrumLogger : IAsyncLogger, ISyncLogger
    {
        private readonly ILogClient _logClient;
        private readonly ISyncLogger _queue;

        /// <param name="baseUri">Where the log service is located.</param>
        /// <param name="authenticationCredentials">Credentials to the log service.</param>
        public FulcrumLogger(string baseUri, ServiceClientCredentials authenticationCredentials)
        {
            InternalContract.RequireNotNullOrWhiteSpace(baseUri, nameof(baseUri));
            InternalContract.RequireNotNull(authenticationCredentials, nameof(authenticationCredentials));
            _logClient = new LogClient(baseUri, authenticationCredentials);
            _queue = new QueueToAsyncLogger(this);
        }

        /// <param name="logClient">The log client</param>
        public FulcrumLogger(ILogClient logClient)
        {
            InternalContract.RequireNotNull(logClient, nameof(logClient));
            _logClient = logClient;
            _queue = new QueueToAsyncLogger(this);
        }

        /// <summary>
        /// Not implemented, use <see cref="LogAsync"/> instead.
        /// </summary>
        public void Log(LogSeverityLevel logSeverityLevel, string message)
        {
            throw new FulcrumNotImplementedException("Obsolete");
        }

        /// <inheritdoc />
        public async Task LogAsync(LogRecord logRecord)
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
            await _logClient.LogAsync(tenant, logMessage);
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            _queue.LogSync(logRecord);
        }
    }
}
