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
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Logic
{
    public delegate Task<TMethodReturnType> ActivityMethod<TMethodReturnType>(
        Activity activity,
        CancellationToken cancellationToken);
    public delegate Task ActivityMethod(
        Activity activity,
        CancellationToken cancellationToken);

    public abstract class Activity : IActivity
    {
        private readonly IInternalActivityFlow _activityFlow;
        private readonly ActivityExecutor _activityExecutor;
        public IDictionary<string, JToken> ContextDictionary => Instance.ContextDictionary;

        protected internal WorkflowCache WorkflowCache => _activityFlow.WorkflowCache;

        /// <inheritdoc />
        public string WorkflowInstanceId => WorkflowInformation.InstanceId;

        /// <inheritdoc />
        public string ActivityInstanceId { get; internal set; }

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
        public int? Iteration { get; protected set; }

        /// <inheritdoc />
        public ActivityOptions Options => _activityFlow.Options;

        /// <inheritdoc />
        [Obsolete("Please use Options.FailUrgency. Compilation warning since 2021-11-19.")]
        public ActivityFailUrgencyEnum FailUrgency => Options.FailUrgency;

        public WorkflowInformation WorkflowInformation => _activityFlow.WorkflowInformation;

        protected internal ActivityForm Form => WorkflowCache.GetActivityForm(_activityFlow.ActivityFormId);
        public ActivityVersion Version => WorkflowCache.GetActivityVersionByFormId(_activityFlow.ActivityFormId);
        public ActivityInstance Instance => WorkflowCache.GetActivityInstance(ActivityInstanceId);
        public string NestedPosition { get; }
        public string NestedPositionAndTitle => $"{NestedPosition} {_activityFlow.FormTitle}";

        /// <inheritdoc />
        public override string ToString() => ActivityTitle;

        /// <inheritdoc />
        public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null, CancellationToken cancellationToken = default)
        {
            try
            {
                FulcrumAssert.IsNotNull(WorkflowInformation, CodeLocation.AsString());
                FulcrumAssert.IsNotNull(WorkflowInformation.StateCapability, CodeLocation.AsString());
                FulcrumAssert.IsNotNullOrWhiteSpace(message, nameof(message));
                if ((int) severityLevel < (int) Options.LogCreateThreshold) return;
                var jToken = WorkflowStatic.SafeConvertToJToken(data);
                var log = new LogCreate
                {
                    WorkflowFormId = WorkflowInformation.FormId,
                    WorkflowInstanceId = WorkflowInstanceId,
                    ActivityFormId = Form?.Id,
                    SeverityLevel = severityLevel,
                    Message = message,
                    Data = jToken,
                    TimeStamp = DateTimeOffset.UtcNow,
                };
                await WorkflowInformation.StateCapability.Log.CreateAsync(log, cancellationToken);
            }
            catch (Exception)
            {
                if (FulcrumApplication.IsInDevelopment) throw;
                // Ignore logging problems when not in development mode.
            }
        }

        public List<int> NestedIterations { get; } = new();

        [Obsolete("Please use Options.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
        public double AsyncRequestPriority => Options.AsyncRequestPriority;

        [Obsolete("Please use Options.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
        public ActivityExceptionAlertHandler ExceptionAlertHandler => Options.ExceptionAlertHandler;

        protected Activity(ActivityTypeEnum activityType, IInternalActivityFlow activityFlow)
        {
            InternalContract.RequireNotNull(activityFlow, nameof(activityFlow));
            _activityFlow = activityFlow;
            Activity parentActivity = null;
            if (WorkflowStatic.Context.ParentActivityInstanceId != null)
            {
                parentActivity = WorkflowCache.GetActivity(WorkflowStatic.Context.ParentActivityInstanceId);
                FulcrumAssert.IsNotNull(parentActivity, CodeLocation.AsString());
                NestedIterations.AddRange(parentActivity.NestedIterations);
                if (parentActivity.Iteration is > 0)
                {
                    NestedIterations.Add(parentActivity.Iteration.Value);
                }
                NestedPosition = $"{parentActivity.NestedPosition}.{activityFlow.Position}";
            }
            else
            {
                NestedPosition = $"{activityFlow.Position}";
            }

            ActivityInstanceId = _activityFlow.WorkflowCache.GetOrCreateInstanceId(activityType, _activityFlow, parentActivity);
            _activityExecutor = new ActivityExecutor(activityFlow.WorkflowInformation.WorkflowImplementation, this);
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
            return _activityFlow.MethodHandler.GetArgument<T>(parameterName);
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

        internal Task<TMethodReturnType> InternalExecuteAsync<TMethodReturnType>(
            ActivityMethod<TMethodReturnType> method,
            Func<CancellationToken, Task<TMethodReturnType>> getDefaultValueMethodAsync,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            return _activityExecutor.ExecuteAsync(method, getDefaultValueMethodAsync, cancellationToken);
        }

        internal async Task InternalExecuteAsync(
            ActivityMethod method,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            await _activityExecutor.ExecuteAsync(method, cancellationToken);
        }

        public async Task PurgeLogsAsync(CancellationToken cancellationToken)
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
            await WorkflowInformation.StateCapability.Log.DeleteActivityChildrenAsync(WorkflowInstanceId, Form.Id, Options.LogPurgeThreshold, cancellationToken);
        }
    }
}