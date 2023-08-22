using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Messages;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

namespace Nexus.Link.Dashboards.Sdk;

public static class StartupExtensions
{
    /// <summary>
    /// Sets up the <see cref="AfterSaveDelegate"/> for workflows to be saved in the Workflow Inspector database.
    /// </summary>
    public static void UseNexusLinkDashboardWithWorkflows(this IApplicationBuilder app,
        IAsyncRequestClient asyncRequestClient,
        IWorkflowInstanceService workflowInstanceService,
        Uri workflowInspectorUrl,
        string apiKey)
    {
        InternalContract.RequireNotNull(FulcrumApplication.Setup.Tenant, nameof(FulcrumApplication.Setup.Tenant));
        InternalContract.RequireNotNullOrWhiteSpace(FulcrumApplication.Setup.ClientName,
            nameof(FulcrumApplication.Setup.ClientName), "Please setup FulcrumApplication.Setup.ClientName");
        InternalContract.RequireNotNull(asyncRequestClient, nameof(asyncRequestClient));
        InternalContract.RequireNotNull(workflowInstanceService, nameof(workflowInstanceService));
        InternalContract.RequireNotNull(workflowInspectorUrl, nameof(workflowInspectorUrl));
        InternalContract.RequireNotNull(apiKey, nameof(apiKey));

        // Trigger when
        // - !Halted => Halted
        // - Halted => !Halted
        workflowInstanceService.DefaultWorkflowOptions.AfterSaveAsync +=
            async (oldForm, oldVersion, oldInstance, newForm, newVersion, newInstance) =>
            {
                if (newInstance == null || oldInstance == null) return;

                var publish = (oldInstance.State != WorkflowStateEnum.Halted && newInstance.State == WorkflowStateEnum.Halted) ||
                              (oldInstance.State == WorkflowStateEnum.Halted && newInstance.State != WorkflowStateEnum.Halted);
                if (!publish) return;

                var url = new Uri(workflowInspectorUrl, "api/v1/WorkflowInstances/Changed");
                var request = asyncRequestClient
                    .CreateRequest(HttpMethod.Post, url.ToString(), 0.5)
                    .SetContentAsJson(new WorkflowInstanceChangedV1
                    {
                        Form = newForm,
                        Version = newVersion,
                        Instance = newInstance,
                        SourceClientId = FulcrumApplication.Setup.ClientName,
                        Tenant = FulcrumApplication.Setup.Tenant,
                        Timestamp = DateTimeOffset.Now
                    })
                    .AddHeader("X-API-KEY", apiKey);
                try
                {
                    await asyncRequestClient.SendRequestAsync(request);
                }
                catch (Exception e)
                {
                    Log.LogError($"Couldn't create async request: {e.Message}. Giving up.");
                }
            };
    }
}