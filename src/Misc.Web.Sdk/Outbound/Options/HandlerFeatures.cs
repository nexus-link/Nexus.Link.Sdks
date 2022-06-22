using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound.Options
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
        /// Logs the request and the response
        /// </summary>
        public LogRequestAndResponseOptions LogRequestAndResponse { get; } = new();

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