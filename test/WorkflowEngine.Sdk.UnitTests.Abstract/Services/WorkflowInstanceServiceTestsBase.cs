using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
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
            var workflowVersionId = Guid.NewGuid().ToString();
            var instanceId = Guid.NewGuid().ToString();
            var itemToCreate = new WorkflowInstanceCreate
            {
                WorkflowVersionId = workflowVersionId,
                InitialVersion = Guid.NewGuid().ToString(),
                StartedAt = DateTimeOffset.Now,
                Title = Guid.NewGuid().ToString()
            };

            // Act
            await _workflowInstanceService.CreateChildWithSpecifiedIdAsync(workflowVersionId, instanceId, itemToCreate);
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
