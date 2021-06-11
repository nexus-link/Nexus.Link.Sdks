using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    public class TokenCache
    {
        private readonly Dictionary<string, AuthenticationToken> _cache = new Dictionary<string, AuthenticationToken>();

        public void AddOrUpdate(string type, IAuthenticationCredentials credentials, AuthenticationToken token)
        {
            lock (_cache)
            {
                _cache[CacheKey(type, credentials)] = token;
            }
        }

        public AuthenticationToken Get(string type, IAuthenticationCredentials credentials, TimeSpan minimumExpirationSpan)
        {
            lock (_cache)
            {
                _cache.TryGetValue(CacheKey(type, credentials), out var token);
                if (token == null) return null;
                if (HasExpired(token))
                {
                    _cache.Remove(CacheKey(type, credentials));
                    return null;
                }
                return WillLive(token, minimumExpirationSpan) ? token : null;
            }
        }

        private static bool HasExpired(AuthenticationToken token)
        {
            return DateTimeOffset.Now >= token.ExpiresOn;
        }

        private static bool WillLive(AuthenticationToken token, TimeSpan timeSpan)
        {
            return token.ExpiresOn >= DateTimeOffset.Now.Add(timeSpan);
        }

        private static string CacheKey(string type, IAuthenticationCredentials credentials)
        {
            return $"{type}{credentials.ClientId}{credentials.ClientSecret}";
        }


    }
}
