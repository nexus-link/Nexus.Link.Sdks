using System.Net;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.Serialization;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Extensions.State
{
    internal static class AsyncManagerExtensions
    {
        public static ResponseData From(this ResponseData target, HttpResponse source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireNotNull(source.HttpStatus, $"{nameof(source)}.{nameof(source.HttpStatus)}");
            target.BodyAsString = source.Content;
            target.Headers = source.Headers;
            target.StatusCode = (HttpStatusCode)source.HttpStatus!.Value;
            target.ContentType = "application/json";
            target.ContentLength = target.BodyAsString?.Length;
            return target;
        }

        public static HttpRequestCreate From(this HttpRequestCreate target, RequestData source)
        {
            InternalContract.RequireNotNull(source, nameof(source));

            target.Url = source.EncodedUrl;
            target.Method = source.Method;
            target.Headers = source.Headers;
            target.Content = source.BodyAsString;

            return target;
        }
    }
}