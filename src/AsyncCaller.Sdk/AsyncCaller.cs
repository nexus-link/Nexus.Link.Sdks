using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.AsyncCaller.Sdk.Data.Models;
using Nexus.Link.AsyncCaller.Sdk.RestClients.Facade;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

#pragma warning disable 1591

namespace Nexus.Link.AsyncCaller.Sdk
{
    public class AsyncCaller : IAsyncCaller
    {
        public ServiceClientCredentials ServiceClientCredentials { get; }

        protected Tenant Tenant;
        private readonly Uri _serviceUri;
        protected string Id;
        private readonly object _lockObject = new object();
        private IAsyncCallsClient _restClient;

        [SuppressMessage("ReSharper", "ReadAccessInDoubleCheckLocking")]
        [SuppressMessage("ReSharper", "PossibleMultipleWriteAccessInDoubleCheckLocking")]
        private IAsyncCallsClient RestClient
        {
            get
            {
                if (_restClient != null) return _restClient;
                lock (_lockObject)
                {
                    if (_restClient != null) return _restClient;
                    
                    _restClient = new AsyncCallsClient(_serviceUri.AbsoluteUri, Tenant, ServiceClientCredentials);
                    return _restClient;
                }
            }
        }

        public AsyncCaller(Uri serviceUri, Tenant tenant, ServiceClientCredentials serviceClientCredentials)
        {
            InternalContract.RequireNotNull(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(tenant, nameof(tenant));

            _serviceUri = serviceUri;
            Tenant = tenant;
            ServiceClientCredentials = serviceClientCredentials;
            Id = Guid.NewGuid().ToString();
        }

        public AsyncCaller(string uri, Tenant tenant, ServiceClientCredentials serviceClientCredentials)
            : this(uri == null ? null : new Uri(uri), tenant, serviceClientCredentials)
        {
            InternalContract.Require(uri, u => Uri.IsWellFormedUriString(u, UriKind.Absolute), nameof(uri));
        }

        public IAsyncCall CreateCall(HttpMethod method, string uri)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.Require(uri, u => Uri.IsWellFormedUriString(u, UriKind.Absolute), nameof(uri));

            var asyncCall = new AsyncCall(this, method, uri);
            return asyncCall;
        }

        public IAsyncCall CreateCall(HttpMethod method, string uri, int priority)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.Require(uri, u => Uri.IsWellFormedUriString(u, UriKind.Absolute), nameof(uri));

            var asyncCall = new AsyncCall(this, method, uri)
                .SetPriority(priority);
            return asyncCall;
        }

        public IAsyncCall CreateCall(HttpMethod method, Uri uri)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.RequireNotNull(uri, nameof(uri));

            var asyncCall = new AsyncCall(this, method, uri);
            return asyncCall;
        }

        public IAsyncCall CreateCall(HttpMethod method, Uri uri, int priority)
        {
            InternalContract.RequireNotNull(method, nameof(method));
            InternalContract.RequireNotNull(uri, nameof(uri));

            var asyncCall = new AsyncCall(this, method, uri)
                .SetPriority(priority);
            return asyncCall;
        }

        public virtual async Task<string> ExecuteAsync(RawRequest rawRequest, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(rawRequest, nameof(rawRequest));
            return await RestClient.PostRawAsync(rawRequest, cancellationToken);
        }
    }
}
