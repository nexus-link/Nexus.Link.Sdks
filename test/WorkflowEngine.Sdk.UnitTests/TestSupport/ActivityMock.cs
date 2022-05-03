using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Context;
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
            var valueProvider = new AsyncLocalContextValueProvider();
            _internalIteration = new OneValueProvider<int?>(valueProvider, nameof(InternalIteration));
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

        private readonly OneValueProvider<int?> _internalIteration;

        /// <inheritdoc />
        public int? InternalIteration
        {
            get => _internalIteration.GetValue();
            set => _internalIteration.SetValue(value);
        }

        /// <inheritdoc />
        [Obsolete($"Please use {nameof(IParentActivity.ChildCounter)}.", true)]
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
}