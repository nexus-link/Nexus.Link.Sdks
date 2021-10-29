﻿using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Crud.Web.RestClient;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.WorkflowEngine.Sdk.RestClients.State
{

    public class WorkflowSummaryRestClient : CrudRestClient<WorkflowSummary, string>, IWorkflowSummaryService
    {
        public WorkflowSummaryRestClient(IHttpSender httpSender) : base(httpSender.CreateHttpSender("Workflows"))
        {
        }
    }
}