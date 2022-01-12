using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class ActivityVersionServiceTestsBase
    {
        private readonly IActivityVersionService _service;

        protected ActivityVersionServiceTestsBase(IActivityVersionService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var parentId = Guid.NewGuid().ToGuidString();
            var activityFormId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToGuidString()
            };

            // Act
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = createdItem?.Id;
            var readItem = await _service.ReadAsync(childId);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(parentId, readItem.WorkflowVersionId);
            Assert.Equal(itemToCreate.Position, readItem.Position);
            Assert.Equal(itemToCreate.ActivityFormId, readItem.ActivityFormId);
            Assert.Equal(itemToCreate.ParentActivityVersionId, readItem.ParentActivityVersionId);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var parentId = Guid.NewGuid().ToGuidString();
            var activityFormId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToGuidString()
            };
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = createdItem?.Id;
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act
            itemToUpdate.Position = 2;
            itemToUpdate.FailUrgency = ActivityFailUrgencyEnum.Ignore;
            await _service.UpdateAndReturnAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToUpdate.Position, readItem.Position);
            Assert.Equal(itemToUpdate.FailUrgency, readItem.FailUrgency);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var parentId = Guid.NewGuid().ToGuidString();
            var activityFormId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityVersionCreate
            {
                WorkflowVersionId = parentId,
                Position = 1,
                ActivityFormId = activityFormId,
                ParentActivityVersionId = Guid.NewGuid().ToGuidString()
            };
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = createdItem?.Id;
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act & Assert
            itemToUpdate.Position = 2;
            itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _service.UpdateAndReturnAsync(childId, itemToUpdate));
        }
    }
}
