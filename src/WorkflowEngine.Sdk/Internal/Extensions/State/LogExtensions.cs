﻿using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Log = Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities.Log;
using LogRecord = Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities.LogRecord;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State
{
    internal static class LogExtensions
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
            target.SeverityLevelNumber = (int) source.SeverityLevel;
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

            target.Id = source.Id.ToGuidString();
            target.Etag = source.Etag;
            target.WorkflowFormId = source.WorkflowFormId.ToGuidString();
            target.WorkflowInstanceId = source.WorkflowInstanceId?.ToGuidString();
            target.ActivityFormId = source.ActivityFormId?.ToGuidString();
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