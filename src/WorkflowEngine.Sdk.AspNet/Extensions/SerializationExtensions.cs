using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.AspNet.Serialization;
using Nexus.Link.Libraries.Web.Serialization;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Nexus.Link.WorkflowEngine.Sdk.AspNet.Extensions
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serialize to <see cref="HttpResponseMessage"/>.
        /// </summary>
        public static async Task<HttpRequestCreate> FromAsync(this HttpRequestCreate target, HttpRequest source, double priority, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var requestData = await new RequestData().FromAsync(source, cancellationToken);
            target.Method = requestData.Method;
            target.Url = requestData.EncodedUrl;
            target.Metadata.Priority = priority;
            target.Headers = CopyWithoutContentHeaders(source.Headers);
            target.Content = requestData.BodyAsString;
            target.ContentType = requestData.ContentType;
            return target;
        }

        private static Dictionary<string, StringValues> CopyWithoutContentHeaders(IHeaderDictionary sourceHeaders)
        {
            return sourceHeaders.Where(h => !h.Key.ToLowerInvariant().StartsWith("content-")).ToDictionary(v => v.Key, v => v.Value);
        }
    }
}