using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Misc;
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
            var id = Guid.NewGuid().ToGuidString();
            var workflowInstanceId = Guid.NewGuid().ToGuidString();
            var activityVersionId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ParentIteration = 1, 
            };

            // Act
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var readItem = await _service.ReadAsync(id);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(createdItem.Id, readItem.Id);
            Assert.Equal(workflowInstanceId, readItem.WorkflowInstanceId);
            Assert.Equal(itemToCreate.ActivityVersionId, readItem.ActivityVersionId);
            Assert.Equal(itemToCreate.ParentActivityInstanceId, readItem.ParentActivityInstanceId);
            Assert.Equal(itemToCreate.ParentIteration, readItem.ParentIteration);
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
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ParentIteration = 1, 
            };
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(id);

            // Act
            itemToUpdate.FinishedAt = DateTimeOffset.Now;
            itemToUpdate.State = ActivityStateEnum.Failed;
            itemToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.TechnicalError;
            itemToUpdate.ExceptionFriendlyMessage =  Guid.NewGuid().ToGuidString();
            itemToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToGuidString();
            var updatedItem = await _service.UpdateAndReturnAsync(createdItem.Id, itemToUpdate);

            // Assert
            Assert.Equal(createdItem.Id, updatedItem.Id);
            Assert.Equal(workflowInstanceId, updatedItem.WorkflowInstanceId);
            Assert.Equal(itemToUpdate.ActivityVersionId, updatedItem.ActivityVersionId);
            Assert.Equal(itemToUpdate.ParentActivityInstanceId, updatedItem.ParentActivityInstanceId);
            Assert.Equal(itemToUpdate.FinishedAt, updatedItem.FinishedAt);
            Assert.Equal(itemToUpdate.ParentIteration, updatedItem.ParentIteration);
            Assert.Equal(itemToUpdate.State, updatedItem.State);
            Assert.Equal(itemToUpdate.ExceptionCategory, updatedItem.ExceptionCategory);
            Assert.Equal(itemToUpdate.ExceptionFriendlyMessage, updatedItem.ExceptionFriendlyMessage);
            Assert.Equal(itemToUpdate.ExceptionTechnicalMessage, updatedItem.ExceptionTechnicalMessage);
        }

        [Fact]
        public async Task Can_Set_To_Success()
        {
            // Arrange
            var item = await CreateActivityInstance();

            // Assert the arrangements
            Assert.Equal(ActivityStateEnum.Executing, item.State);
            Assert.Null(item.ResultAsJson);
            Assert.Null(item.FinishedAt);

            // Act
            var result = new ActivityInstanceSuccessResult { ResultAsJson = "{}"};
            await _service.SuccessAsync(item.Id, result);
            var updatedItem = await _service.ReadAsync(item.Id);

            // Assert
            Assert.Equal(result.ResultAsJson, updatedItem.ResultAsJson);
            Assert.Equal(ActivityStateEnum.Success, updatedItem.State);
            Assert.NotNull(updatedItem.FinishedAt);
        }

        [Fact]
        public async Task Can_Set_To_Failed()
        {
            // Arrange
            var item = await CreateActivityInstance();

            // Assert the arrangements
            Assert.Equal(ActivityStateEnum.Executing, item.State);
            Assert.Null(item.ResultAsJson);
            Assert.Null(item.FinishedAt);

            // Act
            var result = new ActivityInstanceFailedResult
            {
                ExceptionCategory = ActivityExceptionCategoryEnum.BusinessError,
                ExceptionFriendlyMessage = "Speak, friend",
                ExceptionTechnicalMessage = "VagueSpeechException"
            };
            await _service.FailedAsync(item.Id, result);
            var updatedItem = await _service.ReadAsync(item.Id);

            // Assert
            Assert.Equal(result.ExceptionCategory, updatedItem.ExceptionCategory);
            Assert.Equal(result.ExceptionFriendlyMessage, updatedItem.ExceptionFriendlyMessage);
            Assert.Equal(result.ExceptionTechnicalMessage, updatedItem.ExceptionTechnicalMessage);
            Assert.Equal(ActivityStateEnum.Failed, updatedItem.State);
            Assert.NotNull(updatedItem.FinishedAt);
        }

        private async Task<ActivityInstance> CreateActivityInstance()
        {
            var id = Guid.NewGuid().ToGuidString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var activityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToString(),
                ParentIteration = 1,
            };
            var item = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            return item;
        }
    }
}
