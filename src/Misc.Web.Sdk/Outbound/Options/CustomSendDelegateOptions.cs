using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Web.Pipe;

namespace Misc.Web.Sdk.Outbound.Options
{
    /// <summary>
    /// Use a custom send delegate instead of the default one. This comes handy for testing.
    /// </summary>
    public class CustomSendDelegateOptions : Feature, IValidatable
    {
        public delegate Task<HttpResponseMessage> SendAsyncMethod(HttpRequestMessage request, CancellationToken cancellationToken);

        [Validation.NotNull(TriggerPropertyName = nameof(Enabled))]
        public SendAsyncMethod SendAsyncDelegate { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}