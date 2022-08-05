using Nexus.Link.Libraries.Web.Error;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Support
{
    /// <summary>
    /// The names for context variables
    /// </summary>
    public class NexusContextNames
    {
        /// <summary>
        /// Key for setting up user authorization token on async local context
        /// </summary>
        public const string NexusUserAuthorizationKeyName = "NexusUserAuthorization";

        /// <summary>
        /// Key for setting up translated user id on async local context
        /// </summary>
        public const string TranslatedUserIdKey = "NexusTranslatedUserId";
    }
}
