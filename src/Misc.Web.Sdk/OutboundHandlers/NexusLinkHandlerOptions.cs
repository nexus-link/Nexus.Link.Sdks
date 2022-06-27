using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers
{
    /// <summary>
    /// Options that controls <see cref="NexusLinkHandler"/>
    /// </summary>
    public class NexusLinkHandlerOptions : IValidatable
    {
        /// <summary>
        /// The features that can be controlled.
        /// </summary>
        public HandlerFeatures Features = new HandlerFeatures();

        /// <summary>
        /// Set the default values as recommended by Nexus Link
        /// </summary>
        public NexusLinkHandlerOptions SetNexusLinkDefaults()
        {
            // Forward headers
            Features.ForwardNexusUserAuthorization.Enabled = true;
            Features.ForwardNexusTranslatedUserId.Enabled = true;
            Features.ForwardNexusTestContext.Enabled = true;
            Features.ForwardCorrelationId.Enabled = true;

            // Convert HTTP status codes to FulcrumException
            Features.ThrowFulcrumExceptionOnFail.Enabled = true;

            // Log request and response
            Features.LogRequestAndResponse.Enabled = true;

            // Handle execution information
            Features.SaveExecutionInformation.Enabled = true;
            Features.SaveExecutionInformation.SaveBeforeExecutionAsyncDelegate = null;
            Features.SaveExecutionInformation.SaveAfterExecutionAsyncDelegate = null;

            // This is up to the implementor
            Features.CustomSendDelegate.Enabled = false;

            return this;
        }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}