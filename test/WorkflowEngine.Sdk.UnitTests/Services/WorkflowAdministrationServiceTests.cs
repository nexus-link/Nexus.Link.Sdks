using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Services
{
    public class WorkflowAdministrationServiceTests : WorkflowServiceTestsBases
    {
        private readonly IWorkflowAdministrationService _service;

        public WorkflowAdministrationServiceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _service = new WorkflowAdministrationService(WorkflowService, Mock.Of<IRequestResponseService>());
        }


        [Fact]
        public async Task The_Convenience_Workflow_Contains_All_Information_And_Hierarchy()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToString();

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
    }
}