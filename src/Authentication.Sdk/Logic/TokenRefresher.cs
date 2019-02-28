using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    public class TokenRefresher : ServiceClientCredentials, ITokenRefresherWithServiceClient
    {
        private readonly AuthenticationManager _authenticationManager;
        private readonly IAuthenticationCredentials _credentials;
        private readonly TimeSpan _minimumTimeSpan;
        private readonly TimeSpan _maximumTimeSpan;
        private readonly TimeSpan _refreshTimeSpan;
        private readonly object _lockObject = new object();
        private bool _refreshInProgress;

        internal TokenRefresher(AuthenticationManager authenticationManager, IAuthenticationCredentials credentials, TimeSpan minimumTimeSpan, TimeSpan maximumTimeSpan, TimeSpan refreshTimeSpan)
        {
            InternalContract.Require(maximumTimeSpan, x => x >= minimumTimeSpan, $"maximumTimeSpan ({maximumTimeSpan}) was smaller than minimumTimeSpan ({minimumTimeSpan})");

            _authenticationManager = authenticationManager;
            _credentials = credentials;
            _minimumTimeSpan = minimumTimeSpan;
            _maximumTimeSpan = maximumTimeSpan;
            _refreshTimeSpan = refreshTimeSpan;
        }

        internal TokenRefresher(AuthenticationManager authenticationManager, IAuthenticationCredentials credentials, TimeSpan minimumTimeSpan, TimeSpan maximumTimeSpan)
            : this(authenticationManager, credentials, minimumTimeSpan, maximumTimeSpan, CalculateRefreshTimeSpan(minimumTimeSpan, maximumTimeSpan))
        {
        }

        private static TimeSpan CalculateRefreshTimeSpan(TimeSpan minimumTimeSpan, TimeSpan maximumTimeSpan)
        {
            var minHours = minimumTimeSpan.TotalHours;
            var maxHours = maximumTimeSpan.TotalHours;
            var hours = minHours + (maxHours-minHours) / 10.0;
            return TimeSpan.FromHours(hours);
        }

        internal TokenRefresher(AuthenticationManager authenticationManager, IAuthenticationCredentials credentials)
            : this(authenticationManager, credentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24), TimeSpan.FromHours(2))
        {
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync()
        {
            var token = await _authenticationManager.GetJwtTokenAsync(_credentials, _minimumTimeSpan, _maximumTimeSpan);
            if (TokenNeedsRefresh(token)) RefreshTokenInBackground();
            return token;
        }

        public ServiceClientCredentials GetServiceClient()
        {
            return this;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(request, nameof(request));

            var token = await GetJwtTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        private void RefreshTokenInBackground()
        {
            lock (_lockObject)
            {
                if (_refreshInProgress) return;
                _refreshInProgress = true;
            }
            try
            {
                ThreadHelper.FireAndForgetWithExpensiveStackTracePreservation(
                    async () => await _authenticationManager.RequestAndCacheJwtTokenAsync(_credentials, _maximumTimeSpan));
            }
            finally
            {
                lock (_lockObject)
                {
                    _refreshInProgress = false;
                }
            }
        }

        private bool TokenNeedsRefresh(AuthenticationToken cachedToken)
        {
            return !IsTokenValidInTheFuture(cachedToken, _refreshTimeSpan);
        }

        private static bool IsTokenValidInTheFuture(AuthenticationToken token, TimeSpan futureTimeSpan)
        {
            return token.ExpiresOn > DateTimeOffset.Now.Add(futureTimeSpan);
        }
    }
}