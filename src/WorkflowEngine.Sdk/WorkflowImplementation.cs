using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk
{
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

        public WorkflowExecutor WorkflowExecutor { get; set; }

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
        protected T GetArgument<T>(string name)
        {
            return WorkflowExecutor.GetArgument<T>(name);
        }

        /// <inheritdoc />
        protected WorkflowImplementation(int majorVersion, int minorVersion, IWorkflowVersions workflowVersions)
            : base(majorVersion, minorVersion, workflowVersions)
        {
        }

        public abstract IWorkflowImplementation<TWorkflowResult> CreateWorkflowInstance();

        public Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return WorkflowExecutor.ExecuteAsync<TWorkflowResult>(this, cancellationToken);
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