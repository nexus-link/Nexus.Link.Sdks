using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>
    /// </summary>
    public class ForwardExecutionInformationOptions : Feature, IValidatable
    {
        /// <summary>
        /// True: Copy context parent to header parent, context execution to header execution
        /// False: Copy context execution to header parent, context child to header execution
        /// </summary>
        public bool SimpleForward { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}