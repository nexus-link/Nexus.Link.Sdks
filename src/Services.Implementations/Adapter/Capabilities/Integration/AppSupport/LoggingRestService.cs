﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Services.Contracts.Capabilities.Integration.AppSupport;

namespace Nexus.Link.Services.Implementations.Adapter.Capabilities.Integration.AppSupport
{
    /// <inheritdoc cref="ILoggingService" />
    public class LoggingRestService : RestClientBase, ILoggingService
    {
        
        /// <inheritdoc cref="ILoggingService"/>
        public LoggingRestService(IHttpSender httpSender)
            :base(httpSender.CreateHttpSender("Logs"))
        {
        }

        /// <inheritdoc />
        public Task LogAsync(JToken message, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(message, nameof(message));
            return RestClient.PostNoResponseContentAsync<JToken>("", message, null, token);
        }
    }
}
