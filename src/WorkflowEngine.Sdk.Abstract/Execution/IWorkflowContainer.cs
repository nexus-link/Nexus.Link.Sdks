using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution
{
    /// <summary>
    /// A container for all workflow implementations for a specific workflow
    /// </summary>
    public interface IWorkflowContainer
    {
        /// <summary>
        /// The capabilities that are required by the workflow engine
        /// </summary>
        IWorkflowEngineRequiredCapabilities WorkflowCapabilities { get; }

        /// <summary>
        /// The name of the capability that this workflow resides in
        /// </summary>
        string WorkflowCapabilityName { get; }

        /// <summary>
        /// The identity of this workflow container
        /// </summary>
        string WorkflowFormId { get; }

        /// <summary>
        /// The title of this workflow
        /// </summary>
        string WorkflowFormTitle { get; }

        /// <summary>
        /// Get the activity definition for a specific activity, i.e. the activity type and title.
        /// </summary>
        /// <param name="activityFormId">The identifier for the activity</param>
        ActivityDefinition GetActivityDefinition(string activityFormId);

        /// <summary>
        /// Add a <paramref name="workflowImplementation"/> to this container.
        /// </summary>
        void AddImplementation(IWorkflowImplementationBase workflowImplementation);
    }
}