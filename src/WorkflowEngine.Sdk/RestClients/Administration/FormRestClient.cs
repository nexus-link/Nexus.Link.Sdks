﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Components.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IFormOverviewService" />
    public class FormRestClient : RestClient, IFormService, IFormOverviewService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FormRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Forms"))
        {
        }

        /// <inheritdoc />
        public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, CancellationToken cancellationToken = default)
        {
            InternalContract.Require(instancesTo > instancesFrom, $"{nameof(instancesTo)} must be greater than {instancesFrom}");

            var relativeUrl = $"?{nameof(instancesFrom)}={WebUtility.UrlEncode(instancesFrom.ToString("yyyy-MM-dd'T'HH:mm"))}" +
                              $"&{nameof(instancesTo)}={WebUtility.UrlEncode(instancesTo.ToString("yyyy-MM-dd'T'HH:mm"))}";
            return await GetAsync<List<WorkflowFormOverview>>(relativeUrl, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var relativeUrl = WebUtility.UrlEncode(id);
            return await GetAsync<WorkflowForm>(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
