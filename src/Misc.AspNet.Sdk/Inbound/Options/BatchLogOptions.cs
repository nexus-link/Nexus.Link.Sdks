using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// If one log message in a batch has a severity level equal to or higher than <see name="LogAllThreshold"/>,
    /// then all the logs within that batch will be logged, regardless of the value of
    /// <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
    /// </summary>
    public class BatchLogOptions : Feature, IValidatable
    {
        /// <summary>
        /// The threshold for logging all messages within a batch.
        /// </summary>
        public LogSeverityLevel Threshold { get; set; } = LogSeverityLevel.Warning;

        /// <summary>
        /// True means that the records will be released at the end of the batch.
        /// False means that they will be released as soon as one message hits the threshold and then all messages will be released instantly until the batch ends.
        /// </summary>
        public bool FlushAsLateAsPossible { get; set; } = false;

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}