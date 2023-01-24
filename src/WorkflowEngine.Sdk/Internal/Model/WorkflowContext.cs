using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Model
{
    internal class WorkflowContext
    {
        public IContextValueProvider ValueProvider { get; }

        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<string> _workflowInstanceId;
        private readonly OneValueProvider<string> _iterationTitle;
        private readonly OneValueProvider<Activity> _parentActivity;
        private readonly OneValueProvider<Activity> _latestActivity;
        private readonly OneValueProvider<WorkflowExecutor> _currentWorkflowExecutor;

        public WorkflowContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _workflowInstanceId = new OneValueProvider<string>(ValueProvider, "WorkflowInstanceId");
            _iterationTitle = new OneValueProvider<string>(ValueProvider, "IterationTitle");
            _parentActivity = new OneValueProvider<Activity>(ValueProvider, "ParentActivity");
            _latestActivity = new OneValueProvider<Activity>(ValueProvider, "LatestActivity");
            _currentWorkflowExecutor = new OneValueProvider<WorkflowExecutor>(ValueProvider, "CurrentWorkflowExecutor");
        }

        /// <summary>
        /// If this is true, then the current execution is in an truly asynchronous context,
        /// i.e. the client is not waiting for the response, so we are for instance
        /// free to make asynchronous calls to other servers.
        /// </summary>
        public bool ExecutionIsAsynchronous
        {
            get => _executionIsAsynchronous.GetValue();
            set => _executionIsAsynchronous.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains the information about the current execution id.
        /// </summary>
        public string WorkflowInstanceId
        {
            get => _workflowInstanceId.GetValue();
            set => _workflowInstanceId.SetValue(value);
        }
        
        /// <summary>
        /// Represents a description of child process in more details.
        /// </summary>
        public string IterationTitle
        {
            get => _iterationTitle.GetValue();
            set => _iterationTitle.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains information about the current parent id.
        /// </summary>
        public Activity ParentActivity
        {
            get => _parentActivity.GetValue();
            set => _parentActivity.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains information about the current parent id.
        /// </summary>
        public Activity LatestActivity
        {
            get => _latestActivity.GetValue();
            set => _latestActivity.SetValue(value);
        }

        public WorkflowExecutor CurrentWorkflowExecutor
        {
            get => _currentWorkflowExecutor.GetValue();
            set => _currentWorkflowExecutor.SetValue(value);
        }
    }
}