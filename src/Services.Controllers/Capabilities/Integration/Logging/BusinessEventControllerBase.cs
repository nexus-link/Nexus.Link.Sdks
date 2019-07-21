using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Services.Contracts.Capabilities.Integration.BusinessEvents;
using Nexus.Link.Services.Contracts.Capabilities.Integration.Logging;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.Logging
{
    /// <summary>
    /// Service implementation of <see cref="IBusinessEventService"/>
    /// </summary>
    public abstract class LogsControllerBase : ControllerBase, ILoggingService
    {
        protected readonly ILoggingCapability Capability;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability">The logic layer</param>
        protected LogsControllerBase(ILoggingCapability capability)
        {
            Capability = capability;
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("")]
        [Authorize(Roles = "business-api-caller")]
        public virtual Task LogAsync(JToken message, CancellationToken token = default(CancellationToken))
        {
            ServiceContract.RequireNotNull(message, nameof(message));
            ServiceContract.Require(message.Type == JTokenType.Object, $"The {nameof(message)} parameter must be a JSON object.");
            return Capability.LoggingService.LogAsync(message, token);
        }
    }
}
