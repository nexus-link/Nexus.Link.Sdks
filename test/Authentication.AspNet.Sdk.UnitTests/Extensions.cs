#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Nexus.Link.Libraries.Web.Pipe;
using AuthenticationToken = Nexus.Link.Libraries.Core.Platform.Authentication.AuthenticationToken;

namespace Authentication.AspNet.Sdk.UnitTests
{
    internal static class Extensions
    {
        public static HttpRequest SetRequestWithReentryAuthentication(this DefaultHttpContext context, string reentryAuthentication, AuthenticationToken token = null)
        {
            var request = new DefaultHttpRequest(context)
            {
                Scheme = "https",
                Host = new HostString("host.example.com"),
                PathBase = new PathString("/"),
                Path = new PathString("/Person/123"),
                Method = "Get",
                Body = null,
                ContentLength = 0,
                QueryString = new QueryString("?id=23")
            };
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