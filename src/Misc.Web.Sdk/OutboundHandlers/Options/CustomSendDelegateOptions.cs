using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// Use a custom send delegate instead of the default one. This comes handy for testing.
    /// </summary>
    public class CustomSendDelegateOptions : Feature, IValidatable
    {
        /// <summary>
        /// The delegate type for the send async method.
        /// </summary>
        public delegate Task<HttpResponseMessage> SendAsyncMethod(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// The custom delegate to use instead of the next HTTP SendAsync method.
        /// </summary>
        [Validation.NotNull(TriggerPropertyName = nameof(Enabled))]
        public SendAsyncMethod SendAsyncDelegate { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}