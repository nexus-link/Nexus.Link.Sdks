using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class WorkflowServiceSummaryTests : WorkflowServiceTestsBases
    {
        public WorkflowServiceSummaryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        [Fact]
        public async Task The_Convenience_Workflow_Contains_All_Information_And_Hierarchy()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToString();

            // Act
            var workflow = await WorkflowSummaryService.ReadAsync(id);
            TestOutputHelper.WriteLine(JsonConvert.SerializeObject(workflow, Formatting.Indented));

            // Assert
            workflow.Form.Id.ShouldBe(WorkflowFormRecord.Id.ToString());
            workflow.Version.Id.ShouldBe(WorkflowVersionRecord.Id.ToString());
            workflow.Instance.Id.ShouldBe(id);
            workflow.ActivityTree.Count.ShouldBe(2);
            foreach (var activity in workflow.ActivityTree)
            {
                activity.Instance.WorkflowInstanceId.ShouldBe(WorkflowInstanceRecord.Id.ToString());
                activity.Children.Count.ShouldBe(1);
                activity.Children[0].Instance.ParentActivityInstanceId.ShouldBe(activity.Instance.Id);
            }
        }
    }
}
