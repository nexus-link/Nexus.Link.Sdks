﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IFormOverviewService" />
    public class FormRestClient : RestClient, IFormService, IFormOverviewService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FormRestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        /// <inheritdoc />
        public async Task<IList<WorkflowFormOverview>> ReadByIntervalWithPagingAsync(DateTimeOffset instancesFrom, DateTimeOffset instancesTo, FormOverviewIncludeFilter filter, CancellationToken cancellationToken = default)
        {
            InternalContract.Require(instancesTo > instancesFrom, $"{nameof(instancesTo)} must be greater than {instancesFrom}");

            var relativeUrl = $"FormOverviews" +
                              $"?{nameof(instancesFrom)}={WebUtility.UrlEncode(instancesFrom.ToString("yyyy-MM-dd'T'HH:mm"))}" +
                              $"&{nameof(instancesTo)}={WebUtility.UrlEncode(instancesTo.ToString("yyyy-MM-dd'T'HH:mm"))}" +
                              $"&{nameof(filter)}={WebUtility.UrlEncode(filter.ToString())}";
            return await GetAsync<List<WorkflowFormOverview>>(relativeUrl, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkflowForm> ReadAsync(string id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));

            var relativeUrl = $"Forms/{WebUtility.UrlEncode(id)}";
            return await GetAsync<WorkflowForm>(relativeUrl, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<WorkflowForm>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            var relativeUrl = $"Forms?offset={offset}";
            if (limit.HasValue) relativeUrl += $"&limit={limit}";
            return await GetAsync<PageEnvelope<WorkflowForm>>(relativeUrl, cancellationToken: cancellationToken);
        }
    }
}
