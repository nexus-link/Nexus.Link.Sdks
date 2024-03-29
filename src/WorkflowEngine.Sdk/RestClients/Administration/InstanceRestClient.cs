﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Component.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.Administration
{
    /// <inheritdoc cref="IInstanceService" />
    public class InstanceRestClient : CrudRestClient<WorkflowForm, string>, IInstanceService
    {
        /// <summary>
        /// Controller
        /// </summary>
        public InstanceRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Instances"))
        {
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<WorkflowInstance>> SearchAsync(WorkflowInstanceSearchDetails searchDetails, int offset = 0, int? limit = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(searchDetails, nameof(searchDetails));
            InternalContract.RequireValidated(searchDetails, nameof(searchDetails));

            var relativeUrl = $"?offset={offset}";
            if (limit.HasValue) relativeUrl += $"&limit={limit}";
            return await PostAsync<PageEnvelope<WorkflowInstance>, WorkflowInstanceSearchDetails>(relativeUrl, searchDetails, cancellationToken: cancellationToken);
        }
    }
}
