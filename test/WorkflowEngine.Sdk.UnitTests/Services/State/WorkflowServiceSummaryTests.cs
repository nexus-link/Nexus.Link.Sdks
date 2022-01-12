using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;

namespace WorkflowEngine.Sdk.UnitTests.Services.State
{
    public class WorkflowServiceSummaryTests : WorkflowServiceTestsBases
    {
        [TestMethod]
        public async Task The_Convenience_Workflow_Contains_All_Information_And_Hierarchy()
        {
            // Arrange
            var id = WorkflowInstanceRecord.Id.ToGuidString();

            // Act
            var workflow = await WorkflowSummaryService.GetSummaryAsync(id);

            // Assert
            workflow.Form.Id.ShouldBe(WorkflowFormRecord.Id.ToGuidString());
            workflow.Version.Id.ShouldBe(WorkflowVersionRecord.Id.ToGuidString());
            workflow.Instance.Id.ShouldBe(id);
            workflow.ActivityTree.Count.ShouldBe(2);
            foreach (var activity in workflow.ActivityTree)
            {
                activity.Instance.WorkflowInstanceId.ShouldBe(WorkflowInstanceRecord.Id.ToGuidString());
                activity.Children.Count.ShouldBe(1);
                activity.Children[0].Instance.ParentActivityInstanceId.ShouldBe(activity.Instance.Id);
            }
        }
    }
}
