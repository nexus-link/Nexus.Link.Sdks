using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.WorkflowEngine.Sdk
{

    public enum PurgeLogStrategyEnum
    {
        /// <summary>
        /// No purge of log messages
        /// </summary>
        None,
        /// <summary>
        /// Purge logs associated with an activity immediately after it has succeeded.
        /// Purge all logs associated with the workflow immediately after it has completed.
        /// </summary>
        AfterActivitySuccess,
        /// <summary>
        /// Purge all logs associated with the workflow immediately after it has succeeded (even if it hasn't completed yet).
        /// </summary>
        AfterWorkflowSuccess,
        /// <summary>
        /// Purge all logs associated with the workflow immediately after it has succeeded or failed (even if it hasn't completed yet).
        /// </summary>
        AfterWorkflowReturn,
        /// <summary>
        /// Purge all logs associated with the workflow immediately after it has completed, i.e. has no outstanding activities.
        /// </summary>
        AfterWorkflowComplete,
    }
    /// <summary>
    /// Options for running a workflow instance.
    /// </summary>
    public class ActivityOptions
    {
        /// <summary>
        /// If an activity fails, how should this affect the workflow? <see cref="ActivityFailUrgencyEnum"/>.
        /// </summary>
        public ActivityFailUrgencyEnum FailUrgency { get; set; } = ActivityFailUrgencyEnum.Stopping;

        /// <summary>
        /// When an activity throws an exception, the alert handler is called. It gives the workflow programmer
        /// a possibility to publish an event or similar.
        /// </summary>
        public ActivityExceptionAlertHandler ExceptionAlertHandler { get; set; }

        /// <summary>
        /// This value defines the priority for any background asynchronous requests.
        /// </summary>
        public double AsyncRequestPriority { get; set; } = 0.5;

        /// <summary>
        /// This level of logs (and higher) will be saved to the Log table.
        /// </summary>
        public LogSeverityLevel LogSeverityLevelThreshold { get; set; } = LogSeverityLevel.Warning;

        /// <summary>
        /// The strategy to be used after each activity. Purge the activity logs or not?
        /// </summary>
        public PurgeLogStrategyEnum PurgeLogStrategy { get; set; } = PurgeLogStrategyEnum.AfterActivitySuccess;
    }
}