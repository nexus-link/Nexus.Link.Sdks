using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk
{
    /// <inheritdoc />
    public abstract class WorkflowImplementationBase : IWorkflowImplementationBase
    {
        private readonly WorkflowExecutor _workflowExecutor;

        internal IInternalActivity CurrentParentActivity => WorkflowStatic.Context.ParentActivity;

        /// <inheritdoc />
        public int MajorVersion { get; }

        /// <inheritdoc />
        public int MinorVersion { get; }

        /// <inheritdoc />
        public abstract string GetInstanceTitle();

        /// <inheritdoc />
        public IWorkflowContainer WorkflowContainer { get; }

        /// <summary>
        /// The default options for all created activities.
        /// </summary>
        public ActivityOptions DefaultActivityOptions { get; } = new();

        /// <inheritdoc />
        public void SetDebugMode()
        {
            InternalContract.Require(FulcrumApplication.IsInDevelopment, $"Only use {nameof(SetDebugMode)} when you are in development.");
            DefaultActivityOptions.AsyncRequestPriority = 1.0;
            DefaultActivityOptions.LogCreateThreshold = LogSeverityLevel.Verbose;
            DefaultActivityOptions.LogPurgeStrategy = LogPurgeStrategyEnum.None;
            DefaultActivityOptions.LogPurgeThreshold = LogSeverityLevel.None;
            DefaultActivityOptions.MaxTotalRunTimeSpan = TimeSpan.FromHours(1);
            DefaultActivityOptions.PostponeAfterTimeSpan = TimeSpan.FromHours(1);
            DefaultActivityOptions.ActivityMaxExecutionTimeSpan = null;
        }

        /// <inheritdoc />
        public CancellationToken ReducedTimeCancellationToken { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected WorkflowImplementationBase(int majorVersion, int minorVersion, IWorkflowContainer workflowContainer, CancellationToken cancellationToken)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            WorkflowContainer = workflowContainer;

            _workflowExecutor = new WorkflowExecutor(new WorkflowInformation(this), WorkflowContainer.WorkflowCapabilities);
        }

        /// <summary>
        /// Create one activity for the workflow implementation.
        /// </summary>
        /// <param name="position">The relative position in the hierarchy of activities.</param>
        /// <param name="id">The key for the activity form</param>
        /// <typeparam name="TActivityReturns">The type that this activity returns.</typeparam>
        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return _workflowExecutor.CreateActivity<TActivityReturns>(position, id);
        }

        /// <summary>
        /// Create one activity for the workflow implementation.
        /// </summary>
        /// <param name="position">The relative position in the hierarchy of activities.</param>
        /// <param name="id">The key for the activity form</param>
        public IActivityFlow CreateActivity(int position, string id)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            return _workflowExecutor.CreateActivity(position, id);
        }

        /// <summary>
        /// Create one activity for the workflow implementation.
        /// </summary>
        /// <param name="position">The relative position in the hierarchy of activities.</param>
        /// <param name="title">The title for this activity.</param>
        /// <param name="id">The key for the activity form</param>
        /// <typeparam name="TActivityReturns">The type that this activity returns.</typeparam>
        [Obsolete("Please use CreateActivity(position, id) and add a DefineActivity() in your WorkflowContainer. Warning since 2021-12-07.")]
        public IActivityFlow<TActivityReturns> CreateActivity<TActivityReturns>(int position, string title, string id)
        {
            var tmp = WorkflowContainer as WorkflowContainer;
            FulcrumAssert.IsNotNull(tmp, CodeLocation.AsString());
            if (tmp!.GetActivityDefinition(id) == null)
            {
                tmp.DefineActivity(id, title, ActivityTypeEnum.Action);
            }

            return CreateActivity<TActivityReturns>(position, id);
        }

        /// <summary>
        /// Create one activity for the workflow implementation.
        /// </summary>
        /// <param name="position">The relative position in the hierarchy of activities.</param>
        /// <param name="title">The title for this activity.</param>
        /// <param name="id">The key for the activity form</param>
        [Obsolete("Please use CreateActivity(position, id) and add a DefineActivity() in your WorkflowContainer. Warning since 2021-12-07.")]
        public IActivityFlow CreateActivity(int position, string title, string id)
        {
            var tmp = WorkflowContainer as WorkflowContainer;
            FulcrumAssert.IsNotNull(tmp, CodeLocation.AsString());
            if (tmp!.GetActivityDefinition(id) == null)
            {
                tmp.DefineActivity(id, title, ActivityTypeEnum.Action);
            }

            return CreateActivity(position, id);
        }

        /// <summary>
        /// Define one parameter for this workflow
        /// </summary>
        /// <typeparam name="T">The type of this parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        public void DefineParameter<T>(string name)
        {
            _workflowExecutor.DefineParameter<T>(name);
        }

        /// <summary>
        /// Get the value of a workflow parameter.
        /// </summary>
        /// <typeparam name="T">The type of this parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        [Obsolete("Please use GetWorkflowArgument(). Compilation warning since 2021-11-18.")]
        protected T GetArgument<T>(string name)
        {
            return GetWorkflowArgument<T>(name);
        }

        /// <summary>
        /// Get the value of a workflow parameter.
        /// </summary>
        /// <typeparam name="T">The type of this parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        protected T GetWorkflowArgument<T>(string name)
        {
            return _workflowExecutor.GetArgument<T>(name);
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Please use DefaultActivityOptions.FailUrgency. Compilation warning since 2021-11-19.")]
        public void SetDefaultFailUrgency(ActivityFailUrgencyEnum failUrgency)
        {
            _workflowExecutor.SetDefaultFailUrgency(failUrgency);
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Please use DefaultActivityOptions.ExceptionAlertHandler. Compilation warning since 2021-11-19.")]
        public void SetDefaultExceptionAlertHandler(ActivityExceptionAlertMethodAsync alertMethodAsync)
        {
            _workflowExecutor.SetDefaultExceptionAlertHandler(alertMethodAsync);
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Please use DefaultActivityOptions.AsyncRequestPriority. Compilation warning since 2021-11-19.")]
        public void SetDefaultAsyncRequestPriority(double priority)
        {
            _workflowExecutor.SetDefaultAsyncRequestPriority(priority);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{WorkflowContainer} {MajorVersion}.{MinorVersion}";

        /// <inheritdoc />
        public Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data = null,
            CancellationToken cancellationToken = default)
        {
            return _workflowExecutor.LogAtLevelAsync(severityLevel, message, data, cancellationToken);
        }

        internal void InternalSetParameter<TParameter>(string name, TParameter value)
        {
            _workflowExecutor.SetParameter(name, value);
        }

        internal Task<TWorkflowResult> InternalExecuteAsync<TWorkflowResult>(WorkflowImplementation<TWorkflowResult> workflowImplementation, CancellationToken cancellationToken)
        {
            var limitedTimeCancellationToken = new CancellationTokenSource(DefaultActivityOptions.MaxTotalRunTimeSpan);
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitedTimeCancellationToken.Token);
            ReducedTimeCancellationToken = mergedToken.Token;
            return _workflowExecutor.ExecuteAsync(workflowImplementation, cancellationToken);
        }

        internal Task InternalExecuteAsync(WorkflowImplementation workflowImplementation, CancellationToken cancellationToken)
        {
            var limitedTimeCancellationToken = new CancellationTokenSource(DefaultActivityOptions.MaxTotalRunTimeSpan);
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitedTimeCancellationToken.Token);
            ReducedTimeCancellationToken = mergedToken.Token;
            return _workflowExecutor.ExecuteAsync(workflowImplementation, cancellationToken);
        }
    }


    /// <summary>
    /// All workflow implementations should inherit from this class or <see cref="WorkflowImplementation"/>
    /// </summary>
    public abstract class WorkflowImplementation : WorkflowImplementationBase, IWorkflowImplementation
    {

        /// <inheritdoc />
        protected WorkflowImplementation(int majorVersion, int minorVersion, IWorkflowContainer workflowContainer, CancellationToken cancellationToken = default)
            : base(majorVersion, minorVersion, workflowContainer, cancellationToken)
        {
        }

        /// <summary>
        /// This is the factory method that will be used whenever we need to create a new instance of the implementation.
        /// You are expected to call your own constructor and return that instance.
        /// </summary>
        public abstract IWorkflowImplementation CreateWorkflowInstance();

        /// <inheritdoc />
        public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(this, cancellationToken);
        }

        /// <summary>
        /// This is the main method for the implementation. This is where you put all the logic for your workflow implementation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public abstract Task ExecuteWorkflowAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public IWorkflowImplementation SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
            return this;
        }
    }

    /// <summary>
    /// All workflow implementations should inherit from this class or <see cref="WorkflowImplementation"/>
    /// </summary>
    /// <typeparam name="TWorkflowResult">
    /// The type for the result value for this workflow.
    /// </typeparam>
    public abstract class WorkflowImplementation<TWorkflowResult> : WorkflowImplementationBase, IWorkflowImplementation<TWorkflowResult>
    {
        /// <inheritdoc />
        protected WorkflowImplementation(int majorVersion, int minorVersion, IWorkflowContainer workflowContainer, CancellationToken cancellationToken = default)
            : base(majorVersion, minorVersion, workflowContainer, cancellationToken)
        {
        }

        /// <summary>
        /// This is the factory method that will be used whenever we need to create a new instance of the implementation.
        /// You are expected to call your own constructor and return that instance.
        /// </summary>
        public abstract IWorkflowImplementation<TWorkflowResult> CreateWorkflowInstance();

        /// <inheritdoc />
        public virtual Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return InternalExecuteAsync(this, cancellationToken);
        }

        /// <summary>
        /// This is the main method for the implementation. This is where you put all the logic for your workflow implementation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public abstract Task<TWorkflowResult> ExecuteWorkflowAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public IWorkflowImplementation<TWorkflowResult> SetParameter<TParameter>(string name, TParameter value)
        {
            InternalSetParameter(name, value);
            return this;
        }
    }
}