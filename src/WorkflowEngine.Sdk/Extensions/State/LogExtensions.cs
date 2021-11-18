using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Log = Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State.Log;
using LogRecord = Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities.LogRecord;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions.State
{
    public static class LogExtensions
    {
        /// <summary>
        /// LogRecord.From(Log)
        /// </summary>
        public static LogRecordCreate From(this LogRecordCreate target, LogCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.WorkflowFormId = source.WorkflowFormId.ToGuid();
            target.WorkflowInstanceId = source.WorkflowInstanceId?.ToGuid();
            target.ActivityFormId = source.ActivityFormId?.ToGuid();
            target.SeverityLevel = source.SeverityLevel.ToString();
            target.Message = source.Message;
            target.DataAsJson = source.Data?.ToString(Formatting.Indented);
            target.TimeStamp = source.TimeStamp;
            return target;
        }

        /// <summary>
        /// LogRecord.From(Log)
        /// </summary>
        public static LogRecord From(this LogRecord target, Log source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((LogRecordCreate)target).From(source);
            target.Id = source.Id.ToGuid();
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// Log.From(LogRecord)
        /// </summary>
        public static Log From(this Log target, LogRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            target.Id = source.Id.ToLowerCaseString();
            target.Etag = source.Etag;
            target.WorkflowFormId = source.WorkflowFormId.ToLowerCaseString();
            target.WorkflowInstanceId = source.WorkflowInstanceId?.ToLowerCaseString();
            target.ActivityFormId = source.ActivityFormId?.ToLowerCaseString();
            target.SeverityLevel = source.SeverityLevel.ToEnum<LogSeverityLevel>();
            target.Message = source.Message;
            target.Data = source.DataAsJson == null ? null : JToken.Parse(source.DataAsJson);
            target.TimeStamp = source.TimeStamp;
            return target;
        }

        public static Task LogCriticalAsync(this IWorkflowLogger activityLogger, string message, object data,
            CancellationToken cancellationToken = default)
        {
            return activityLogger.LogAtLevelAsync(LogSeverityLevel.Critical, message, data, cancellationToken);
        }

        public static Task LogErrorAsync(this IWorkflowLogger activityLogger, string message, object data,
            CancellationToken cancellationToken = default)
        {
            return activityLogger.LogAtLevelAsync(LogSeverityLevel.Error, message, data, cancellationToken);
        }

        public static Task LogWarningAsync(this IWorkflowLogger activityLogger, string message, object data,
            CancellationToken cancellationToken = default)
        {
            return activityLogger.LogAtLevelAsync(LogSeverityLevel.Warning, message, data, cancellationToken);
        }

        public static Task LogInformationAsync(this IWorkflowLogger activityLogger, string message, object data,
            CancellationToken cancellationToken = default)
        {
            return activityLogger.LogAtLevelAsync(LogSeverityLevel.Information, message, data, cancellationToken);
        }

        public static Task LogVerboseAsync(this IWorkflowLogger activityLogger, string message, object data,
            CancellationToken cancellationToken = default)
        {
            return activityLogger.LogAtLevelAsync(LogSeverityLevel.Verbose, message, data, cancellationToken);
        }
    }
}