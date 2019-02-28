﻿using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    internal class TokenCache
    {
        private readonly Dictionary<string, AuthenticationToken> _cache = new Dictionary<string, AuthenticationToken>();

        public void AddOrUpdate(IAuthenticationCredentials credentials, AuthenticationToken token)
        {
            lock (_cache)
            {
                _cache[CacheKey(credentials)] = token;
            }
        }

        public AuthenticationToken Get(IAuthenticationCredentials credentials, TimeSpan minimumExpirationSpan)
        {
            lock (_cache)
            {
                AuthenticationToken token;
                _cache.TryGetValue(CacheKey(credentials), out token);
                if (token == null) return null;
                if (HasExpired(token))
                {
                    _cache.Remove(CacheKey(credentials));
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

        private static string CacheKey(IAuthenticationCredentials credentials)
        {
            return $"{credentials.ClientId}{credentials.ClientSecret}";
        }


    }
}