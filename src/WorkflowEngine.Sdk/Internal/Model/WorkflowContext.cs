using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Model
{
    internal class WorkflowContext
    {
        public IContextValueProvider ValueProvider { get; }
        
        private readonly OneValueProvider<string> _workflowInstanceId;
        private readonly OneValueProvider<Activity> _parentActivity;
        private readonly OneValueProvider<Activity> _latestActivity;
        private readonly OneValueProvider<WorkflowExecutor> _currentWorkflowExecutor;

        public WorkflowContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _workflowInstanceId = new OneValueProvider<string>(ValueProvider, "WorkflowInstanceId");
            _parentActivity = new OneValueProvider<Activity>(ValueProvider, "ParentActivity");
            _latestActivity = new OneValueProvider<Activity>(ValueProvider, "LatestActivity");
            _currentWorkflowExecutor = new OneValueProvider<WorkflowExecutor>(ValueProvider, "CurrentWorkflowExecutor");
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
        /// If non-null, contains information about the current parent activity.
        /// </summary>
        public Activity ParentActivity
        {
            get => _parentActivity.GetValue();
            set => _parentActivity.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains information about the latest activity.
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