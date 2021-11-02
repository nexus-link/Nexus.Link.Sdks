using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
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
            var workflowVersionId = Guid.NewGuid().ToLowerCaseString();
            var instanceId = Guid.NewGuid().ToLowerCaseString();
            var itemToCreate = new WorkflowInstanceCreate
            {
                WorkflowVersionId = workflowVersionId,
                InitialVersion = Guid.NewGuid().ToLowerCaseString(),
                StartedAt = DateTimeOffset.Now,
                Title = Guid.NewGuid().ToLowerCaseString()
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
