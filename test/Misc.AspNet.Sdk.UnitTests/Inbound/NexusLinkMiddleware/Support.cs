using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Support;

namespace Misc.AspNet.Sdk.UnitTests.Inbound.NexusLinkMiddleware
{
    internal static class Support
    {
        public static void SetRequest(this DefaultHttpContext context, string url = "http://example.com/Person/123?test=1", string method = "GET", JObject jObject = null)
        {
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]*)?(\\?.*)?$");
            if (!match.Success)
            {
                throw new ApplicationException($"The url was formatted unexpectedly: {url}");
            }
            var memoryStream = jObject == null ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(jObject.ToString()));

            context.Request.Scheme = match.Groups[1].Value;
            context.Request.Host = new HostString(match.Groups[2].Value);
            context.Request.PathBase = new PathString("/");
            context.Request.Path = new PathString(match.Groups[3].Value);
            context.Request.Method = method;
            context.Request.Body = memoryStream;
            context.Request.ContentLength = memoryStream.Length;
            context.Request.QueryString = new QueryString(match.Groups[4].Value);
        }
        public static void SetRequestWithReentryAuthentication(this DefaultHttpContext context, string reentryAuthentication)
        {
            var memoryStream = new MemoryStream();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("host.example.com");
            context.Request.PathBase = new PathString("/");
            context.Request.Path = new PathString("/Person/123");
            context.Request.Method = "Get";
            context.Request.Body = memoryStream;
            context.Request.ContentLength = memoryStream.Length;
            context.Request.QueryString = new QueryString("?id=23");
            if (reentryAuthentication != null)
            {
                context.Request.Headers.Add(NexusHeaderNames.ReentryAuthenticationHeaderName, reentryAuthentication);
            }
        }
    }
}
