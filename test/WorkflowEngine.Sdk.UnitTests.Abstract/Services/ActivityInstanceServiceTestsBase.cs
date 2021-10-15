using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowMgmt.UnitTests.Services
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
            var workflowInstanceId = Guid.NewGuid().ToString();
            var activityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToString(),
                ParentIteration = 1, 
            };

            // Act
            var id = await _service.CreateAsync(itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var readItem = await _service.FindUniqueAsync(findUnique);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(id, readItem.Id);
            Assert.Equal(workflowInstanceId, readItem.WorkflowInstanceId);
            Assert.Equal(itemToCreate.ActivityVersionId, readItem.ActivityVersionId);
            Assert.Equal(itemToCreate.ParentActivityInstanceId, readItem.ParentActivityInstanceId);
            Assert.Equal(itemToCreate.ParentIteration, readItem.ParentIteration);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var workflowInstanceId = Guid.NewGuid().ToString();
            var activityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToString(),
                ParentIteration = 1, 
            };
            var id = await _service.CreateAsync(itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var itemToUpdate = await _service.FindUniqueAsync(findUnique);

            // Act
            itemToUpdate.FinishedAt = DateTimeOffset.Now;
            itemToUpdate.FailUrgency = ActivityFailUrgencyEnum.Stopping;
            itemToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.Other;
            itemToUpdate.ExceptionFriendlyMessage =  Guid.NewGuid().ToString();
            itemToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToString();
            await _service.UpdateAsync(id, itemToUpdate);
            var readItem = await _service.FindUniqueAsync(findUnique);

            // Assert
            Assert.Equal(id, readItem.Id);
            Assert.Equal(workflowInstanceId, readItem.WorkflowInstanceId);
            Assert.Equal(itemToUpdate.ActivityVersionId, readItem.ActivityVersionId);
            Assert.Equal(itemToUpdate.ParentActivityInstanceId, readItem.ParentActivityInstanceId);
            Assert.Equal(itemToUpdate.FinishedAt, readItem.FinishedAt);
            Assert.Equal(itemToUpdate.ParentIteration, readItem.ParentIteration);
            Assert.Equal(itemToUpdate.State, readItem.State);
            Assert.Equal(itemToUpdate.FailUrgency, readItem.FailUrgency);
            Assert.Equal(itemToUpdate.ExceptionCategory, readItem.ExceptionCategory);
            Assert.Equal(itemToUpdate.ExceptionFriendlyMessage, readItem.ExceptionFriendlyMessage);
            Assert.Equal(itemToUpdate.ExceptionTechnicalMessage, readItem.ExceptionTechnicalMessage);
        }
    }
}
