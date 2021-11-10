using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.AsyncManager.Sdk.RestClients;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Administration;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.WorkflowEngine.Sdk.Services.Administration;
using Nexus.Link.WorkflowEngine.Sdk.Services.State;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Services.Administration
{
    public class WorkflowServiceTests : WorkflowServiceTestsBases
    {
        private readonly IWorkflowService _service;
        private static Mock<IHttpSender> _httpSenderMock;

        public static IHttpSender HttpSender
        {
            get
            {

                if (_httpSenderMock != null) return _httpSenderMock.Object;
                _httpSenderMock = new Mock<IHttpSender>();
                _httpSenderMock.Setup(sender => sender.SendRequestAsync(
                        It.IsAny<HttpMethod>(),
                        It.IsAny<string>(), null,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync((HttpMethod method, string relativeUrl,
                        Dictionary<string, List<string>> customHeaders,
                        CancellationToken cancellationToken) =>
                    {
                        
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    });
                return _httpSenderMock.Object;
            }
        }

        public WorkflowServiceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var workflowInstanceService = new WorkflowInstanceService(RuntimeTables);
            var asyncCap = new AsyncRequestMgmtRestClients(HttpSender);
            var workflowCap = new Mock<IWorkflowMgmtCapability>();
            workflowCap.Setup(x => x.WorkflowSummary).Returns(WorkflowSummaryService);
            workflowCap.Setup(x => x.WorkflowInstance).Returns(workflowInstanceService);
            
            _service = new WorkflowService(workflowCap.Object, asyncCap);
        }


        [Fact]
        public async Task The_Convenience_Workflow_Contains_All_Information_And_Hierarchy()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToLowerCaseString();

            // Act
            var workflow = await _service.ReadAsync(id);
            TestOutputHelper.WriteLine(JsonConvert.SerializeObject(workflow, Formatting.Indented));

            // Assert
            workflow.Id.ShouldBe(id);
            workflow.Title.ShouldContain(WorkflowFormRecord.Title);
            workflow.Title.ShouldContain($"{WorkflowVersionRecord.MajorVersion}.{WorkflowVersionRecord.MinorVersion}");
            workflow.Title.ShouldContain(WorkflowInstanceRecord.Title);

            workflow.Activities.Count.ShouldBe(2);
            workflow.Activities[0].Position.ShouldBe("1");
            workflow.Activities[0].Children.Count.ShouldBe(1);
            workflow.Activities[0].Children[0].Position.ShouldBe("1.1");
            workflow.Activities[0].Children[0].Children.Count.ShouldBe(1);
            workflow.Activities[0].Children[0].Children[0].Position.ShouldBe("1.1.1");
        }

        [Fact]
        public async Task Cancelling_A_Workflow_Sets_CancelledAt()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToLowerCaseString();

            // Act
            await _service.CancelWorkflowAsync(id);

            // Assert
            var workflow = await _service.ReadAsync(id);
            workflow.CancelledAt.ShouldNotBeNull();
        }
    }
}