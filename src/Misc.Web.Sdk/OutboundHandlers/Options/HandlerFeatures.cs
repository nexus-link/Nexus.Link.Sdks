using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// All the features for <see cref="NexusLinkHandler"/>.
    /// </summary>
    public class HandlerFeatures : IValidatable
    {
        /// <summary>
        /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>.
        /// </summary>
        public ForwardCorrelationIdOptions ForwardCorrelationId { get; } = new();

        /// <summary>
        /// Forward header <see cref="Constants.NexusTranslatedUserIdHeaderName"/>.
        /// </summary>
        public ForwardNexusTranslatedUserIdOptions ForwardNexusTranslatedUserId { get; } = new();

        /// <summary>
        /// Forward header <see cref="Constants.NexusUserAuthorizationHeaderName"/>.
        /// </summary>
        public ForwardNexusUserAuthorizationOptions ForwardNexusUserAuthorization { get; } = new();

        /// <summary>
        /// Forward header <see cref="Constants.NexusUserAuthorizationHeaderName"/>.
        /// </summary>
        public ForwardNexusTestContextOptions ForwardNexusTestContext { get; } = new();

        /// <summary>
        /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>.
        /// </summary>
        public ForwardExecutionInformationOptions ForwardExecutionInformation { get; } = new();

        /// <summary>
        /// Forward headers <see cref="Constants.ExecutionIdHeaderName"/> and <see cref="Constants.ExecutionIdHeaderName"/>
        /// and save information about the call.
        /// </summary>
        public SaveExecutionInformationOptions SaveExecutionInformation { get; } = new();

        /// <summary>
        /// Logs the request and the response
        /// </summary>
        public LogRequestAndResponseOptions LogRequestAndResponse { get; } = new();

        /// <summary>
        /// Logs the request and the response
        /// </summary>
        public RerouteAsynchronousRequestsOptions RerouteAsynchronousRequests { get; } = new();

        /// <summary>
        /// Use a custom send delegate instead of the default one. This comes handy for testing.
        /// </summary>
        public CustomSendDelegateOptions CustomSendDelegate { get; } = new();

        /// <summary>
        /// Any non-successful response will be thrown as a <see cref="FulcrumException"/>.
        /// </summary>
        public ThrowFulcrumExceptionOnFailOptions ThrowFulcrumExceptionOnFail{ get; } = new();

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(ForwardCorrelationId, propertyPath, nameof(ForwardCorrelationId), errorLocation);
        }
    }
}