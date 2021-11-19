using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.WorkflowEngine.Sdk
{
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
    }
}