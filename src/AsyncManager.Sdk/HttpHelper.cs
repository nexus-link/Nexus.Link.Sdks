using System;

namespace Nexus.Link.AsyncManager.Sdk
{
    internal static class HttpHelper
    {

        public static bool IsValidUri(string url)
        {
            var result = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            return result;
        }
    }
}