using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class WorkflowInstanceServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IWorkflowInstanceService _workflowInstanceService;

        protected WorkflowInstanceServiceTestsBase(IWorkflowInstanceService workflowInstanceService)
        {
            _workflowInstanceService = workflowInstanceService;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var workflowVersionId = Guid.NewGuid().ToGuidString();
            var instanceId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new WorkflowInstanceCreate
            {
                WorkflowVersionId = workflowVersionId,
                InitialVersion = Guid.NewGuid().ToGuidString(),
                StartedAt = DateTimeOffset.Now,
                Title = Guid.NewGuid().ToGuidString()
            };

            // Act
            await _workflowInstanceService.CreateWithSpecifiedIdAsync(instanceId, itemToCreate);
            var readItem = await _workflowInstanceService.ReadAsync(instanceId);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(workflowVersionId, readItem.WorkflowVersionId);
            Assert.Equal(instanceId, readItem.Id);
            Assert.Equal(itemToCreate.StartedAt, readItem.StartedAt);
            Assert.Equal(itemToCreate.Title, readItem.Title);
            Assert.Null(readItem.FinishedAt);
        }
    }
}
