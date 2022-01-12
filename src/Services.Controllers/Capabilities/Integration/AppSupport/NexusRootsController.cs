using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Services.Controllers.Capabilities.Integration.AppSupport
{
    /// <summary>
    /// Service implementation of <see cref="IRootService"/>
    /// </summary>
    [ApiController]
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class NexusRootsController : IRootService
    {
        /// <inheritdoc />
        [HttpGet("")]
        [Produces("text/html")]
        public virtual Task<ContentResult> Welcome(CancellationToken cancellationToken = default)
        {
            var result = new ContentResult {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = GetWelcomeHtml()
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Override this if you want to change the HTML welcome message
        /// </summary>
        /// <returns>The HTML to present in the <see cref="Welcome"/> method.</returns>
        protected virtual string GetWelcomeHtml()
        {
            var content =
                "<html>" +
                $"<header><title>{FulcrumApplication.Setup.Name}</title></header>" +
                "<body>" +
                $"Welcome to the API of the application {FulcrumApplication.Setup.Name}.<p/>" +
                "Please visit the <a href=\"/swagger/index.html\">Open API</a>." +
                "</body>" +
                "</html>";
            return content;
        }
    }
}