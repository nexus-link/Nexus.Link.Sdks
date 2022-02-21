using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services
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
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(instanceId);
            readItem.WorkflowVersionId.ShouldBeEquivalentTo(workflowVersionId);
            readItem.StartedAt.ShouldBeEquivalentTo(itemToCreate.StartedAt);
            readItem.Title.ShouldBeEquivalentTo(itemToCreate.Title);
            readItem.FinishedAt.ShouldBeNull();
            readItem.CancelledAt.ShouldBeNull();
        }
    }
}
