using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
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
            var parentId = Guid.NewGuid().ToString();
            var childId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = "Action",
                Title = Guid.NewGuid().ToString()
            };

            // Act
            await _service.CreateWithSpecifiedIdAsync(childId, itemToCreate);
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
            var parentId = Guid.NewGuid().ToString();
            var childId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = "Action",
                Title = Guid.NewGuid().ToString()
            };
            await _service.CreateWithSpecifiedIdAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act
            itemToUpdate.Title = Guid.NewGuid().ToString();
            await _service.UpdateAsync(childId, itemToUpdate);
            var readItem = await _service.ReadAsync(childId);

            // Assert
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToUpdate.Title, readItem.Title);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var parentId = Guid.NewGuid().ToString();
            var childId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityFormCreate
            {
                WorkflowFormId = parentId,
                Type = "Action",
                Title = Guid.NewGuid().ToString()
            };
            await _service.CreateWithSpecifiedIdAsync(childId, itemToCreate);
            var itemToUpdate = await _service.ReadAsync(childId);

            // Act & Assert
            itemToUpdate.Title = Guid.NewGuid().ToString();
            itemToUpdate.Etag = Guid.NewGuid().ToString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _service.UpdateAsync(childId, itemToUpdate));
        }
    }
}
