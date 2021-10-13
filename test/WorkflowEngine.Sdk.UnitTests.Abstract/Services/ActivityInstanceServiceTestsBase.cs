using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
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
            itemToUpdate.ExceptionType = nameof(FulcrumConflictException);
            itemToUpdate.ExceptionMessage = Guid.NewGuid().ToString();
            await _service.UpdateAsync(id, itemToUpdate);
            var readItem = await _service.FindUniqueAsync(findUnique);

            // Assert
            Assert.Equal(id, readItem.Id);
            Assert.Equal(workflowInstanceId, readItem.WorkflowInstanceId);
            Assert.Equal(itemToUpdate.ActivityVersionId, readItem.ActivityVersionId);
            Assert.Equal(itemToUpdate.ParentActivityInstanceId, readItem.ParentActivityInstanceId);
            Assert.Equal(itemToUpdate.FinishedAt, readItem.FinishedAt);
            Assert.Equal(itemToUpdate.ParentIteration, readItem.ParentIteration);
            Assert.Equal(itemToUpdate.ExceptionType, readItem.ExceptionType);
            Assert.Equal(itemToUpdate.ExceptionMessage, readItem.ExceptionMessage);
        }
    }
}
