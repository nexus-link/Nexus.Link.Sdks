using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Shouldly;

namespace Nexus.Link.Capabilities.WorkflowConfiguration.UnitTests.Services
{
    public abstract class ActivityVersionServiceTestsBase
    {
        private readonly IActivityVersionService _service;

        protected ActivityVersionServiceTestsBase(IActivityVersionService service)
        {
            _service = service;
        }

        [TestMethod]
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
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var readItem = await _service.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(childId);
            readItem.WorkflowVersionId.ShouldBeEquivalentTo(parentId);
            readItem.Position.ShouldBeEquivalentTo(itemToCreate.Position);
            readItem.ActivityFormId.ShouldBeEquivalentTo(itemToCreate.ActivityFormId);
            readItem.ParentActivityVersionId.ShouldBeEquivalentTo(itemToCreate.ParentActivityVersionId);
        }

        [TestMethod]
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
                ParentActivityVersionId = Guid.NewGuid().ToGuidString(),
                FailUrgency = ActivityFailUrgencyEnum.Stopping
            };
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var itemToUpdate = await _service.ReadAsync(id);

            // Act
            itemToUpdate.Position = 2;
            itemToUpdate.FailUrgency = ActivityFailUrgencyEnum.Ignore;
            await _service.UpdateAndReturnAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(childId);
            readItem.Position.ShouldBeEquivalentTo(itemToCreate.Position);
            readItem.FailUrgency.ShouldBeEquivalentTo(itemToUpdate.FailUrgency);
        }

        [TestMethod]
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
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                ParentActivityVersionId = Guid.NewGuid().ToGuidString()
            };
            var activityVersion = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var childId = activityVersion.Id;
            var itemToUpdate = await _service.ReadAsync(id);

            // Act & Assert
            itemToUpdate.Position = 2;
            itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _service.UpdateAndReturnAsync(childId, itemToUpdate));
        }
    }
}
