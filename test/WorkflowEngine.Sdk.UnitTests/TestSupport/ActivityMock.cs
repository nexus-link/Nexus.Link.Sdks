using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport
{
    internal class ActivityMock : ActivityBase, IInternalActivity
    {
        /// <inheritdoc />
        public ActivityMock(IActivityInformation activityInformation) : base(activityInformation)
        {
            ActivityStartedAt = DateTimeOffset.UtcNow;
        }

        public int MaybePurgeLogsCalled { get; private set; }
        public int SafeAlertExceptionCalled { get; private set; }

        /// <inheritdoc />
        public Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public string ActivityTitle { get; set; }

        /// <inheritdoc />
        public DateTimeOffset ActivityStartedAt { get; set; }

        /// <inheritdoc />
        [Obsolete($"Please use {nameof(ILoopActivity.LoopIteration)} or {nameof(IParallelActivity.JobNumber)}.", true)]
        public int? Iteration => InternalIteration;

        /// <inheritdoc />
        public T GetArgument<T>(string parameterName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T GetActivityArgument<T>(string parameterName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetContext<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T GetContext<T>(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryGetContext<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void MaybePurgeLogs()
        {
            MaybePurgeLogsCalled++;
        }

        /// <inheritdoc />
        public Task SafeAlertExceptionAsync(CancellationToken cancellationToken)
        {
            SafeAlertExceptionCalled++;
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc cref="ActivityMock" />
    internal class ActivityMock<TActivityReturns> : ActivityMock, IInternalActivity<TActivityReturns>
    {
        public ActivityDefaultValueMethodAsync<TActivityReturns> DefaultValueMethodAsync { get; }

        public ActivityMock(IActivityInformation activityInformation,
            ActivityDefaultValueMethodAsync<TActivityReturns> defaultValueMethodAsync)
            : base(activityInformation)
        {
            DefaultValueMethodAsync = defaultValueMethodAsync;
        }

        public TActivityReturns GetResult()
        {
            return ActivityInformation.Workflow.GetActivityResult<TActivityReturns>(ActivityInstanceId);
        }
    }
}