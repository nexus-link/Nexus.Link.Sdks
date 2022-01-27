using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    public delegate Task<bool> ActivityExceptionAlertHandler(ActivityExceptionAlert alert,
        CancellationToken cancellationToken = default);

    public abstract class WorkflowImplementationBase : IWorkflowImplementationBase
    {
        private readonly WorkflowExecutor WorkflowExecutor;

        /// <inheritdoc />
        public int MajorVersion { get; }

        /// <inheritdoc />
        public int MinorVersion { get; }

        /// <inheritdoc />
        public abstract string GetInstanceTitle();

        /// <inheritdoc />
        public IWorkflowVersions WorkflowVersions { get; }

        public ActivityOptions DefaultActivityOptions => WorkflowExecutor.DefaultActivityOptions;

        public IActivity CurrentParentActivity => WorkflowExecutor.GetCurrentParentActivity();

        protected WorkflowImplementationBase(int majorVersion, int minorVersion, IWorkflowVersions workflowVersions)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            WorkflowVersions = workflowVersions;
            WorkflowExecutor = new WorkflowExecutor(this);
        }
        
        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return WorkflowExecutor.CreateActivity<TActivityReturns>(position, id);
        }
        
        public IActivityFlow CreateActivity(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return WorkflowExecutor.CreateActivity(position, id);
        }

        [Obsolete("Please use CreateActivity(position, id) and add a DefineActivity() in your WorkflowVersions. Warning since 2021-12-07.")]
        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string title, string id)
        {
            var tmp = WorkflowVersions as WorkflowVersions;
            FulcrumAssert.IsNotNull(tmp, CodeLocation.AsString());
            if (tmp!.GetActivityDefinition(id) == null)
            {
                tmp.DefineActivity(id, title, ActivityTypeEnum.Action);
            }

            return CreateActivity<TActivityReturns>(position, id);
        }

        [Obsolete("Please use CreateActivity(position, id) and add a DefineActivity() in your WorkflowVersions. Warning since 2021-12-07.")]
        public IActivityFlow CreateActivity(int position, string title, string id)
        {
            var tmp = WorkflowVersions as WorkflowVersions;
            FulcrumAssert.IsNotNull(tmp, CodeLocation.AsString());
            if (tmp!.GetActivityDefinition(id) == null)
            {
                tmp.DefineActivity(id, title, ActivityTypeEnum.Action);
            }

            return CreateActivity(position, id);
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

        internal void InternalSetParameter<TParameter>(string name, TParameter value)
        {
            WorkflowExecutor.SetParameter(name, value);
        }

        internal Task<TWorkflowResult> InternalExecuteAsync<TWorkflowResult>(WorkflowImplementation<TWorkflowResult> workflowImplementation, CancellationToken cancellationToken)
        {
            return WorkflowExecutor.ExecuteAsync(workflowImplementation, cancellationToken);
        }

        internal Task InternalExecuteAsync(WorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
        {
            return WorkflowExecutor.ExecuteAsync(workflowImplementation, cancellationToken);
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
        public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(this, cancellationToken);
        }

        public abstract Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public IWorkflowImplementation SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
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

        public virtual Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(this, cancellationToken);
        }

        public abstract Task<TWorkflowResult> ExecuteWorkflowAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public IWorkflowImplementation<TWorkflowResult> SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
            return this;
        }
    }
}