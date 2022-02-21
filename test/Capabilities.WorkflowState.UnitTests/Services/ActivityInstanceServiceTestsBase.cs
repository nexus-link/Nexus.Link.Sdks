using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services
{
    public abstract class ActivityInstanceServiceTestsBase
    {
        private readonly IActivityInstanceService _service;

        protected ActivityInstanceServiceTestsBase(IActivityInstanceService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var workflowInstanceId = Guid.NewGuid().ToGuidString();
            var activityVersionId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                StartedAt = DateTimeOffset.UtcNow,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ParentIteration = 1, 
            };

            // Act
            var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var readItem = await _service.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(created.Id);
            readItem.WorkflowInstanceId.ShouldBeEquivalentTo(workflowInstanceId);
            readItem.ActivityVersionId.ShouldBeEquivalentTo(itemToCreate.ActivityVersionId);
            readItem.ParentActivityInstanceId.ShouldBeEquivalentTo(itemToCreate.ParentActivityInstanceId);
            readItem.ParentIteration.ShouldBeEquivalentTo(itemToCreate.ParentIteration);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var workflowInstanceId = Guid.NewGuid().ToGuidString();
            var activityVersionId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                StartedAt = DateTimeOffset.UtcNow,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ParentIteration = 1, 
            };
            var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var itemToUpdate = await _service.ReadAsync(id);

            // Act
            itemToUpdate.FinishedAt = DateTimeOffset.Now;
            itemToUpdate.State = ActivityStateEnum.Failed;
            itemToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.TechnicalError;
            itemToUpdate.ExceptionFriendlyMessage =  Guid.NewGuid().ToGuidString();
            itemToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToGuidString();
            var updatedItem = await _service.UpdateAndReturnAsync(created.Id, itemToUpdate);

            // Assert
            updatedItem.ShouldNotBeNull();
            updatedItem.Id.ShouldBeEquivalentTo(created.Id);
            updatedItem.WorkflowInstanceId.ShouldBeEquivalentTo(created.WorkflowInstanceId);
            updatedItem.ActivityVersionId.ShouldBeEquivalentTo(created.ActivityVersionId);
            updatedItem.ParentActivityInstanceId.ShouldBeEquivalentTo(itemToUpdate.ParentActivityInstanceId);
            updatedItem.ParentIteration.ShouldBeEquivalentTo(itemToUpdate.ParentIteration);
            updatedItem.FinishedAt.ShouldBeEquivalentTo(itemToUpdate.FinishedAt);
            updatedItem.State.ShouldBeEquivalentTo(itemToUpdate.State);
            updatedItem.ExceptionCategory.ShouldBeEquivalentTo(itemToUpdate.ExceptionCategory);
            updatedItem.ExceptionFriendlyMessage.ShouldBeEquivalentTo(itemToUpdate.ExceptionFriendlyMessage);
            updatedItem.ExceptionTechnicalMessage.ShouldBeEquivalentTo(itemToUpdate.ExceptionTechnicalMessage);
        }
    }
}
