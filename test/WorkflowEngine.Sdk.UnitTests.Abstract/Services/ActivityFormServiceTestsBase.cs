using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class ActivityFormServiceTestsBase
    {
        private readonly IActivityFormService _service;

        protected ActivityFormServiceTestsBase(IActivityFormService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToLowerCaseString();
            var childId = Guid.NewGuid().ToLowerCaseString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToLowerCaseString()
            };

            // Act
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(parentId, readItem.WorkflowFormId);
            Assert.Equal(itemToCreate.Type, readItem.Type);
            Assert.Equal(itemToCreate.Title, readItem.Title);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToLowerCaseString();
            var childId = Guid.NewGuid().ToLowerCaseString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToLowerCaseString()
            };
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act
            itemToUpdate.Title = Guid.NewGuid().ToLowerCaseString();
            await _service.UpdateAndReturnAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToUpdate.Title, readItem.Title);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToLowerCaseString();
            var childId = Guid.NewGuid().ToLowerCaseString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = ActivityTypeEnum.Action,
                Title = Guid.NewGuid().ToLowerCaseString()
            };
            await _service.CreateWithSpecifiedIdAndReturnAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act & Assert
            itemToUpdate.Title = Guid.NewGuid().ToLowerCaseString();
            itemToUpdate.Etag = Guid.NewGuid().ToLowerCaseString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _service.UpdateAndReturnAsync(childId, itemToUpdate));
        }
    }
}
