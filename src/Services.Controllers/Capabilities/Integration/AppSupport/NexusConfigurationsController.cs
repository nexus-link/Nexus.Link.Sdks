using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers;
using Nexus.Link.Libraries.Crud.AspNet.Controllers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Service implementation of <see cref="IConfigurationService"/>
    /// </summary>
    [ApiController]
    [Area("AppSupport")]
    [Route("api/Integration/v1/[area]/v1/Configurations")]
    [Authorize(Policy = "HasMandatoryRole")]
    public class NexusConfigurationsController : IConfigurationService
    {
        /// <summary>
        /// The capability where this controller resides
        /// </summary>
        protected IAppSupportCapability Capability { get; }

        /// <summary>
        /// The CrudController for this controller
        /// </summary>
        protected readonly ICrud<JToken, string> CrudController;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capability"></param>
        public NexusConfigurationsController(IAppSupportCapability capability)
        {
            Capability = capability;
            CrudController = new CrudControllerHelper<JToken>(capability.ConfigurationService);
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public async Task<JToken> ReadAsync(string id, CancellationToken token = new CancellationToken())
        {
            var authenticatedSystemName = FulcrumApplication.Context.ClientPrincipal?.Identity.Name;
            FulcrumAssert.IsNotNull(authenticatedSystemName, CodeLocation.AsString());
            if (id.ToLowerInvariant() != authenticatedSystemName?.ToLowerInvariant())
            {
                throw new FulcrumForbiddenAccessException(
                    $"{nameof(id)} ({id}) must be the same as the authenticated client id ({authenticatedSystemName}).");
            }

            try
            {
                return await CrudController.ReadAsync(id, token);
            }
            catch (FulcrumNotFoundException)
            {
                return new JObject();
            }
        }
    }
}
