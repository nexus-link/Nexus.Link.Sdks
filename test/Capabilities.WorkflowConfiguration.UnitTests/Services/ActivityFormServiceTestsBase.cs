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
    public abstract class ActivityFormServiceTestsBase
    {
        private readonly IActivityFormService _service;

        protected ActivityFormServiceTestsBase(IActivityFormService service)
        {
            _service = service;
        }

        [TestMethod]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToGuidString();
            var childId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToGuidString()
            };

            // Act
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(childId);
            readItem.WorkflowFormId.ShouldBeEquivalentTo(parentId);
            readItem.Type.ShouldBeEquivalentTo(itemToCreate.Type);
            readItem.Title.ShouldBeEquivalentTo(itemToCreate.Title);
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToGuidString();
            var childId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToGuidString()
            };
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act
            itemToUpdate.Title = Guid.NewGuid().ToGuidString();
            await _service.UpdateAndReturnAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            readItem.Id.ShouldBeEquivalentTo(childId);
            readItem.Title.ShouldBeEquivalentTo(itemToUpdate.Title);
        }

        [TestMethod]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToGuidString();
            var childId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToGuidString()
            };
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act & Assert
            itemToUpdate.Title = Guid.NewGuid().ToGuidString();
            itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _service.UpdateAndReturnAsync(childId, itemToUpdate));
        }
    }
}
