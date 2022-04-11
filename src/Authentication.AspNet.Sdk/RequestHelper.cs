using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
#if NETCOREAPP
#else
using System.Net.Http;
#endif

namespace Nexus.Link.Authentication.AspNet.Sdk
{
    public class RequestHelper
    {

        // Reads the token from the authorization header on the inbound request
        public static string GetAuthorizationBearerTokenOrApiKey(CompabilityInvocationContext context)
        {
            string token;

            var headerValue = GetRequestHeaderValues("Authorization", context)?.FirstOrDefault();
            if (headerValue != null)
            {
                // Verify Authorization header contains 'Bearer' scheme
                token = headerValue.ToLowerInvariant().StartsWith("bearer ") ? headerValue.Split(' ')[1] : null;
                if (token != null) return token;
            }

            token = GetRequestQueryValues("api_key", context)?.FirstOrDefault();
            return token;
        }
        internal static string GetRequestUserAgent(CompabilityInvocationContext context)
        {
            return GetRequestHeaderValues("User-Agent", context)?.FirstOrDefault();
        }

        internal static IEnumerable<string> GetRequestHeaderValues(string headerName,
            CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
            if (request == null) return null;
            if (!request.Headers.TryGetValue(headerName, out var headerValues)) return null;
            return headerValues;
#else
            var request = context?.RequestMessage;
            if (request == null) return null;

            return !request.Headers.Contains(headerName) ? null : request.Headers.GetValues(headerName);
#endif
        }

        internal static IEnumerable<string> GetRequestQueryValues(string name, CompabilityInvocationContext context)
        {
#if NETCOREAPP
            var request = context?.Context?.Request;
            if (request == null) return null;
            if (!request.Query.TryGetValue(name, out var values)) return null;
            return values;
#else
            var request = context?.RequestMessage;
            return request?.GetQueryNameValuePairs().Where(x => x.Key == name).Select(x => x.Value);
#endif
        }
    }
}
