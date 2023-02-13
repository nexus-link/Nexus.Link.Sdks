#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Web.Pipe;
using AuthenticationToken = Nexus.Link.Libraries.Core.Platform.Authentication.AuthenticationToken;

namespace Authentication.AspNet.Sdk.UnitTests
{
    internal static class Extensions
    {
        public static HttpRequest SetRequestWithReentryAuthentication(this DefaultHttpContext context, string reentryAuthentication, AuthenticationToken token = null)
        {
            var request = context.Request;
            request.Scheme = "https";
            request.Host = new HostString("host.example.com");
            request.PathBase = new PathString("/");
            request.Path = new PathString("/Person/123");
            request.Method = "Get";
            request.Body = null;
            request.ContentLength = 0;
            request.QueryString = new QueryString("?id=23");
            if (reentryAuthentication != null)
            {
                request.Headers.Add(Constants.ReentryAuthenticationHeaderName, reentryAuthentication);
            }

            if (token != null)
            {
                request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
            }
            return request;
        }
    }
}
#endif