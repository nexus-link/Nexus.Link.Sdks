using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncManager.Sdk.UnitTests;
using Moq;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Dashboards.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Memory;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using Xunit;

namespace Dashboards.Sdk.UnitTests
{
    public class StartupExtensionsTests
    {
        public StartupExtensionsTests()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(StartupExtensionsTests));
        }

        [Theory]
        [InlineData(WorkflowStateEnum.Executing, WorkflowStateEnum.Halted)]
        [InlineData(WorkflowStateEnum.Halting, WorkflowStateEnum.Halted)]
        [InlineData(WorkflowStateEnum.Waiting, WorkflowStateEnum.Halted)]
        [InlineData(WorkflowStateEnum.Halted, WorkflowStateEnum.Executing)]
        [InlineData(WorkflowStateEnum.Halted, WorkflowStateEnum.Waiting)]
        [InlineData(WorkflowStateEnum.Halted, WorkflowStateEnum.Success)]
        [InlineData(WorkflowStateEnum.Halted, WorkflowStateEnum.Failed)]
        public async Task Notification_Is_Sent_When_Instance_State_Changes(WorkflowStateEnum oldState, WorkflowStateEnum newState)
        {
            // Arrange
            var amClient = new Mock<IAsyncRequestClient>();
            amClient
                .Setup(x => x.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<double>()))
                .Returns((HttpMethod method, string url, double prio) => new AsyncHttpRequest_ForTest(amClient.Object, method, url, prio));
            amClient
                .Setup(x => x.SendRequestAsync(It.IsAny<AsyncHttpRequest>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var workflowInstanceService = new WorkflowInstanceService(new RuntimeTablesMemory(), new WorkflowOptions());

            StartupExtensions.UseNexusLinkDashboardWithWorkflows(null, amClient.Object, workflowInstanceService, new Uri("http://example.org"), "apikey-foo123");
            workflowInstanceService.DefaultWorkflowOptions.ShouldNotBeNull();

            var oldInstance = new WorkflowInstance { State = oldState };
            var newInstance = new WorkflowInstance { State = newState };

            // Act
            await workflowInstanceService.DefaultWorkflowOptions.AfterSaveAsync(null, null, oldInstance, null, null, newInstance);

            // Assert
            amClient.Verify();
        }
    }
}