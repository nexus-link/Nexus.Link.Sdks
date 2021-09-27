#if NETCOREAPP
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.WorkflowEngine.Sdk.Inbound.RespondAsync.Logic
{
    public abstract class ResponseHandlerBase : IResponseHandler
    {
        protected virtual string UrlFormat { get; }

        protected ResponseHandlerBase(string urlFormat)
        {
            UrlFormat = urlFormat;
        }

        public abstract Task<ResponseData> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default);

        public virtual string GetResponseUrl(Guid requestId)
        {
            var url = string.Format(UrlFormat, requestId.ToString());
            return url;
        }

        /// <inheritdoc />
        public abstract Task AddResponse(RequestData requestData, ResponseData responseData);

        protected ResponseData AcceptedResponse(Guid requestId)
        {
            var redirectObject = new RedirectObject { ResponseUrl = GetResponseUrl(requestId)};
            return new ResponseData
            {
                StatusCode = HttpStatusCode.Accepted,
                BodyAsString = JsonConvert.SerializeObject(redirectObject)
            };
        }

        public Task<IActionResult> GetActionResultAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var redirectObject = new RedirectObject { ResponseUrl = GetResponseUrl(requestId)};
            var result = new ObjectResult(redirectObject)
            {
                StatusCode = StatusCodes.Status202Accepted,
            };
            return Task.FromResult((IActionResult) result);
        }
    }

    public class RedirectObject
    {
        public string ResponseUrl { get; set; }
    }
}
#endif