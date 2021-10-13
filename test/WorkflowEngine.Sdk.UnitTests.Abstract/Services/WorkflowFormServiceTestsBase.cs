using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class WorkflowFormServiceTestsBase
    {
        private readonly IWorkflowFormService _workflowFormService;

        protected WorkflowFormServiceTestsBase(IWorkflowFormService workflowFormService)
        {
            _workflowFormService = workflowFormService;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };

            // Act
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var readItem = await _workflowFormService.ReadAsync(id);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(id, readItem.Id);
            Assert.Equal(itemToCreate.Title, readItem.Title);
            Assert.Equal(itemToCreate.CapabilityName, readItem.CapabilityName);
        }

        [Fact]
        public async Task Create_Given_SameTitle_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var newId = Guid.NewGuid().ToString();

            // Act && Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowFormService.CreateWithSpecifiedIdAsync(newId, itemToCreate));
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var itemToUpdate = await _workflowFormService.ReadAsync(id);

            // Act
            itemToUpdate.Title = Guid.NewGuid().ToString();
            await _workflowFormService.UpdateAsync(id, itemToUpdate);
            var readItem = await _workflowFormService.ReadAsync(id);

            // Assert
            Assert.Equal(id, readItem.Id);
            Assert.Equal(itemToUpdate.Title, readItem.Title);
        }

        [Fact]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var itemToUpdate = await _workflowFormService.ReadAsync(id);

            // Act & Assert
            itemToUpdate.Title = Guid.NewGuid().ToString();
            itemToUpdate.Etag = Guid.NewGuid().ToString();
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowFormService.UpdateAsync(id, itemToUpdate));
        }

        [Fact]
        public async Task Update_Given_ExistingTitle_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid().ToString();
            var itemToCreate1 = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id1, itemToCreate1);
            var id2 = Guid.NewGuid().ToString();
            var itemToCreate2 = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToString(),
                Title = Guid.NewGuid().ToString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id2, itemToCreate2);
            var itemToUpdate = await _workflowFormService.ReadAsync(id2);
            itemToUpdate.CapabilityName = itemToCreate1.CapabilityName;
            itemToUpdate.Title = itemToCreate1.Title;

            // Act & assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowFormService.UpdateAsync(id2, itemToUpdate));
        }
    }
}
