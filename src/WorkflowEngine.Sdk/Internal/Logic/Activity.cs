using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic
{
    /// <inheritdoc />
    internal abstract class Activity : IActivity
    {
        public IActivityInformation ActivityInformation { get; }

        protected IActivityExecutor ActivityExecutor { get; }

        public IDictionary<string, JToken> ContextDictionary => Instance.ContextDictionary;

        /// <inheritdoc />
        public string WorkflowInstanceId => ActivityInformation.Workflow.InstanceId;

        /// <inheritdoc />
        public DateTimeOffset WorkflowStartedAt => ActivityInformation.Workflow.StartedAt;

        /// <inheritdoc />
        public string ActivityInstanceId { get; internal set; }

        public string ActivityFormId => ActivityInformation.FormId;

        /// <inheritdoc />
        public string ActivityTitle
        {
            get
            {
                if (!NestedIterations.Any()) return NestedPositionAndTitle;
                var iterations = string.Join(",", NestedIterations);
                return $"{NestedPositionAndTitle} [{iterations}]";
            }
        }

        /// <inheritdoc />
        public DateTimeOffset ActivityStartedAt => Instance.StartedAt;

        /// <inheritdoc />
        public int? Iteration { get; set; }

        /// <inheritdoc />
        public ActivityOptions Options => ActivityInformation.Options;

        /// <inheritdoc />
        [Obsolete("Please use Options.FailUrgency. Compilation warning since 2021-11-19.")]
        public ActivityFailUrgencyEnum FailUrgency => Options.FailUrgency;

        protected internal ActivityForm Form => ActivityInformation.Workflow.GetActivityForm(ActivityInformation.FormId);
        public ActivityVersion Version => ActivityInformation.Workflow.GetActivityVersionByFormId(ActivityInformation.FormId);
        public ActivityInstance Instance => ActivityInformation.Workflow.GetActivityInstance(ActivityInstanceId);
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {ActivityInformation.FormTitle}";

        /// <inheritdoc />
        public override string ToString() => ActivityTitle;

        public List<int> NestedIterations { get; } = new();

        [Obsolete("Please use Options.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
        public double AsyncRequestPriority => Options.AsyncRequestPriority;

        [Obsolete("Please use Options.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
        public ActivityExceptionAlertHandler ExceptionAlertHandler => Options.ExceptionAlertHandler;

        protected Activity(IActivityInformation activityInformation)
        {
            InternalContract.RequireNotNull(activityInformation, nameof(activityInformation));
            ActivityInformation = activityInformation;
            var parentActivity = ActivityInformation.Workflow.GetCurrentParentActivity();
            if (parentActivity != null)
            {
                NestedIterations.AddRange(parentActivity.NestedIterations);
                if (parentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(parentActivity.Iteration.Value);
                }
                NestedPosition = $"{parentActivity.NestedPosition}.{ActivityInformation.Position}";
            }
            else
            {
                NestedPosition = $"{ActivityInformation.Position}";
            }

            ActivityInstanceId = ActivityInformation.Workflow.GetOrCreateInstanceId(activityInformation);
            ActivityExecutor = ActivityInformation.Workflow.GetActivityExecutor(this);
        }

        /// <inheritdoc />
        public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null, CancellationToken cancellationToken = default)
        {
            if (ActivityInformation.Workflow.LogService == null) return;
            try
            {
                if ((int)severityLevel < (int)Options.LogCreateThreshold) return;
                var jToken = WorkflowStatic.SafeConvertToJToken(data);
                var log = new LogCreate
                {
                    WorkflowFormId = ActivityInformation.Workflow.FormId,
                    WorkflowInstanceId = WorkflowInstanceId,
                    ActivityFormId = Form?.Id,
                    SeverityLevel = severityLevel,
                    Message = message,
                    Data = jToken,
                    TimeStamp = DateTimeOffset.UtcNow,
                };
                ActivityInformation.Logs.Add(log);
            }
            catch (Exception)
            {
                if (FulcrumApplication.IsInDevelopment) throw;
                // Ignore logging problems when not in development mode.
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        [Obsolete("Please use GetActivityArgument(). Compilation warning since 2021-11-18.")]
        public TParameter GetArgument<TParameter>(string parameterName)
        {
            return GetActivityArgument<TParameter>(parameterName);
        }

        /// <inheritdoc />
        public T GetActivityArgument<T>(string parameterName)
        {
            return ActivityInformation.GetArgument<T>(parameterName);
        }

        /// <inheritdoc />
        public void SetContext<T>(string key, T value)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            FulcrumAssert.IsNotNull(ContextDictionary, CodeLocation.AsString());
            ContextDictionary[key] = JToken.FromObject(value);
        }

        /// <inheritdoc />
        public T GetContext<T>(string key)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            FulcrumAssert.IsNotNull(ContextDictionary, CodeLocation.AsString());
            if (!ContextDictionary.ContainsKey(key))
            {
                throw new FulcrumNotFoundException($"Could not find key {key} in context dictionary for activity {ActivityInstanceId}.");
            } 
            var jToken = ContextDictionary[key];
            FulcrumAssert.IsNotNull(jToken, CodeLocation.AsString());
            return jToken.ToObject<T>();
        }

        /// <inheritdoc />
        public bool TryGetContext<T>(string key, out T value)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            value = default;
            if (!ContextDictionary.ContainsKey(key)) return false;
            value = GetContext<T>(key);
            return true;
        }

        public void MaybePurgeLogs(CancellationToken cancellationToken)
        {
            var purge = false;
            switch (Options.LogPurgeStrategy)
            {
                case LogPurgeStrategyEnum.AfterActivitySuccess:
                    purge = Instance.State == ActivityStateEnum.Success;
                    break;
                case LogPurgeStrategyEnum.None:
                case LogPurgeStrategyEnum.AfterWorkflowSuccess:
                case LogPurgeStrategyEnum.AfterWorkflowReturn:
                case LogPurgeStrategyEnum.AfterWorkflowComplete:
                    break;
                default:
                    throw new FulcrumAssertionFailedException(
                        $"Unexpected {nameof(LogPurgeStrategyEnum)}: {Options.LogPurgeStrategy}", 
                        CodeLocation.AsString());
            }

            if (!purge) return;
            foreach (var logCreate in ActivityInformation.Logs)
            {
                if ((int) logCreate.SeverityLevel <= (int) Options.LogPurgeThreshold) continue;
                ActivityInformation.Workflow.Logs.Add(logCreate);
            }
        }

        public Task MarkasFailedAsync(ActivityException exception, CancellationToken cancellationToken = default)
        {
            Instance.State = ActivityStateEnum.Failed;
            Instance.FinishedAt = DateTimeOffset.UtcNow;
            ContextDictionary.Clear();
            Instance.ExceptionCategory = exception.ExceptionCategory;
            Instance.ExceptionTechnicalMessage = exception.TechnicalMessage;
            Instance.ExceptionFriendlyMessage = exception.FriendlyMessage;
            return SafeAlertExceptionAsync(cancellationToken);
        }

        public async Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
        {
            if (Instance.State != ActivityStateEnum.Failed) return;
            if (Instance.ExceptionCategory == null)
            {
                await this.LogErrorAsync($"Instance.ExceptionCategory unexpectedly was null. {CodeLocation.AsString()}",
                    Instance, cancellationToken);
                return;
            }

            if (Instance.ExceptionAlertHandled.HasValue &&
                Instance.ExceptionAlertHandled.Value) return;

            if (Options.ExceptionAlertHandler == null) return;

            var alert = new ActivityExceptionAlert
            {
                Activity = this,
                ExceptionCategory = Instance.ExceptionCategory!.Value,
                ExceptionFriendlyMessage =
                    Instance.ExceptionFriendlyMessage,
                ExceptionTechnicalMessage =
                    Instance.ExceptionTechnicalMessage
            };
            try
            {
                var handled =
                    await Options.ExceptionAlertHandler(alert, cancellationToken);
                Instance.ExceptionAlertHandled = handled;
            }
            catch (Exception)
            {
                // We will try again next reentry.
            }
        }
    }
}