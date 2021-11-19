using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public delegate Task<bool> ActivityExceptionAlertHandler(ActivityExceptionAlert alert,
        CancellationToken cancellationToken = default);

    public abstract class WorkflowImplementationBase : IWorkflowImplementationBase
    {
        /// <inheritdoc />
        public int MajorVersion { get; }

        /// <inheritdoc />
        public int MinorVersion { get; }

        /// <inheritdoc />
        public abstract string GetInstanceTitle();

        /// <inheritdoc />
        public IWorkflowVersions WorkflowVersions { get; }

        protected WorkflowExecutor WorkflowExecutor { get; }

        public ActivityOptions DefaultActivityOptions => WorkflowExecutor.DefaultActivityOptions;

        protected WorkflowImplementationBase(int majorVersion, int minorVersion, IWorkflowVersions workflowVersions)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            WorkflowVersions = workflowVersions;
            WorkflowExecutor = new WorkflowExecutor(this);
        }
        
        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return WorkflowExecutor.CreateActivity<TActivityReturns>(position, title, id);
        }
        
        public IActivityFlow CreateActivity(int position, string title, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(title, nameof(title));
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return WorkflowExecutor.CreateActivity(position, title, id);
        }

        public void DefineParameter<T>(string name)
        {
            WorkflowExecutor.DefineParameter<T>(name);
        }

        
        [Obsolete("Please use GetWorkflowArgument(). Compilation warning since 2021-11-18.")]
        protected T GetArgument<T>(string name)
        {
            return GetWorkflowArgument<T>(name);
        }
        protected T GetWorkflowArgument<T>(string name)
        {
            return WorkflowExecutor.GetArgument<T>(name);
        }

        [Obsolete("Please use DefaultActivityOptions.FailUrgency. Compilation warning since 2021-11-19.")]
        public void SetDefaultFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            WorkflowExecutor.SetDefaultFailUrgency(failUrgency);
        }

        [Obsolete("Please use DefaultActivityOptions.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
        public void SetDefaultExceptionAlertHandler(ActivityExceptionAlertHandler alertHandler)
        {
            WorkflowExecutor.SetDefaultExceptionAlertHandler(alertHandler);
        }

        [Obsolete("Please use DefaultActivityOptions.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
        public void SetDefaultAsyncRequestPriority(double priority)
        {
            WorkflowExecutor.SetDefaultAsyncRequestPriority(priority);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{WorkflowVersions} {MajorVersion}.{MinorVersion}";

        /// <inheritdoc />
        public Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default)
        {
            return WorkflowExecutor.LogAtLevelAsync(severityLevel, message, data, cancellationToken);
        }
    }
    public abstract class WorkflowImplementation : WorkflowImplementationBase, IWorkflowImplementation
    {

        /// <inheritdoc />
        protected WorkflowImplementation(int majorVersion, int minorVersion, IWorkflowVersions workflowVersions)
            : base(majorVersion, minorVersion, workflowVersions)
        {
        }

        public abstract IWorkflowImplementation CreateWorkflowInstance();

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return WorkflowExecutor.ExecuteAsync(this, cancellationToken);
        }

        public abstract Task ExecuteWorkflowAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public IWorkflowImplementation SetParameter<TParameter>(string name, TParameter value)
        {
            WorkflowExecutor.SetParameter(name, value);
            return this;
        }
    }

    public abstract class WorkflowImplementation<TWorkflowResult> : WorkflowImplementationBase, IWorkflowImplementation<TWorkflowResult>
    {
        /// <inheritdoc />
        protected WorkflowImplementation(int majorVersion, int minorVersion, IWorkflowVersions workflowVersions)
            : base(majorVersion, minorVersion, workflowVersions)
        {
        }

        public abstract IWorkflowImplementation<TWorkflowResult> CreateWorkflowInstance();

        public Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return WorkflowExecutor.ExecuteAsync(this, cancellationToken);
        }

        public abstract Task<TWorkflowResult> ExecuteWorkflowAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public IWorkflowImplementation<TWorkflowResult> SetParameter<TParameter>(string name, TParameter value)
        {
            WorkflowExecutor.SetParameter(name, value);
            return this;
        }
    }
}