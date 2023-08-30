using System;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Support
{

    /// <summary>
    /// The accepted values for a log purge strategy.
    /// </summary>
    public enum LogPurgeStrategyEnum
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
        [JsonIgnore]
        public ActivityExceptionAlertMethodAsync ExceptionAlertHandler { get; set; }

        /// <summary>
        /// This value defines the priority for any background asynchronous requests.
        /// </summary>
        public double AsyncRequestPriority { get; set; } = 0.5;

        /// <summary>
        /// Logs with this level of severity level and higher will be saved to the Log table.
        /// </summary>
        public LogSeverityLevel LogCreateThreshold { get; set; } = LogSeverityLevel.Warning;

        /// <summary>
        /// Logs with this severity level or lower will be affected by the <see cref="LogPurgeStrategy"/>.
        /// </summary>
        public LogSeverityLevel LogPurgeThreshold { get; set; } = LogSeverityLevel.Information;

        /// <summary>
        /// The strategy to be used after each activity and workflow. Purge the activity logs or not?
        /// </summary>
        public LogPurgeStrategyEnum LogPurgeStrategy { get; set; } = LogPurgeStrategyEnum.AfterActivitySuccess;

        /// <summary>
        /// If an activity is still executing after this time, it will throw an exception.
        /// </summary>
        public TimeSpan? ActivityMaxExecutionTimeSpan { get; set; }

        /// <summary>
        /// If this method returns true, the exception is ignored.
        /// </summary>
        [Obsolete("This will not be supported. Please use Action+Catch. Obsolete since 2022-06-15.")]
        public ActivityExceptionHandlerAsync ExceptionHandler { get; set; }

        // TODO: Create a new WorkflowOptions and move this option there
        /// <summary>
        /// When an activity is started, the workflow engine postpones the execution if the execution
        /// has passed this time. This is done before calling the actual activity method, to
        /// get an early fail.
        /// </summary>
        public TimeSpan PostponeAfterTimeSpan { get; set; } = TimeSpan.FromSeconds(20.0);

        // TODO: Create a new WorkflowOptions and move this option there
        /// <summary>
        /// The workflow will use this value to limit the total execution time for a single run, to avoid
        /// running into an outer cancellation time. The workflow engine needs some time to
        /// do the absolutely necessary save state after the run.
        /// </summary>
        public TimeSpan MaxTotalRunTimeSpan { get; set; } = TimeSpan.FromSeconds(60.0);

        /// <summary>
        /// Copy the options from <paramref name="source"/>.
        /// </summary>
        public ActivityOptions From(ActivityOptions source)
        {
            FailUrgency = source.FailUrgency;
            ExceptionAlertHandler = source.ExceptionAlertHandler;
            AsyncRequestPriority = source.AsyncRequestPriority;
            LogCreateThreshold = source.LogCreateThreshold;
            LogPurgeThreshold = source.LogPurgeThreshold;
            LogPurgeStrategy = source.LogPurgeStrategy;
#pragma warning disable CS0618
            ExceptionHandler = source.ExceptionHandler;
#pragma warning restore CS0618
            PostponeAfterTimeSpan = source.PostponeAfterTimeSpan;
            MaxTotalRunTimeSpan = source.MaxTotalRunTimeSpan;
            return this;
        }
    }
}