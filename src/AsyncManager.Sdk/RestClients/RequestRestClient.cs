﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.AsyncManager.Sdk.RestClients
{
    /// <inheritdoc />
    public class RequestRestClient : IRequestService
    {
        private readonly IHttpSender _httpSender;

        /// <summary>
        /// Constructor
        /// </summary>
        public RequestRestClient(IHttpSender httpSender)
        {
            _httpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(HttpRequestCreate request, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            InternalContract.RequireValidated(request, nameof(request));

            const string relativeUrl = "Requests";
            var result = await _httpSender.SendRequestAsync<string, HttpRequestCreate>(HttpMethod.Post, relativeUrl, request, null, cancellationToken);
            //Should we really do asserts here? If result.IsSuccessStatusCode is false we'll throw a FulcrumAssertionFailedException with the message 'Expected value to be true'. Feels bad man.

            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            FulcrumAssert.IsNotNull(result.Response, CodeLocation.AsString());
            if (result!.Response.IsSuccessStatusCode != true)
            {
                throw new FulcrumResourceException($"Expected successful statusCode or the httpSender to throw an exception, but received HTTP status {result.Response.StatusCode}.\r" +
                                                   $"We recommend that you use {typeof(ThrowFulcrumExceptionOnFail).FullName} as a handler in your {nameof(IHttpSender)}; it will convert failed HTTP requests to exceptions.");
            }

            return result.Body;
        }
    }
}