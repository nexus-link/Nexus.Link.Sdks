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
    public abstract class WorkflowFormServiceTestsBase
    {
        private readonly IWorkflowFormService _workflowFormService;

        protected WorkflowFormServiceTestsBase(IWorkflowFormService workflowFormService)
        {
            _workflowFormService = workflowFormService;
        }

        [TestMethod]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };

            // Act
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var readItem = await _workflowFormService.ReadAsync(id);

            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(id);
            readItem.Title.ShouldBeEquivalentTo(itemToCreate.Title);
            readItem.CapabilityName.ShouldBeEquivalentTo(itemToCreate.CapabilityName);
        }

        [TestMethod]
        public async Task Create_Given_SameTitle_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var newId = Guid.NewGuid().ToGuidString();

            // Act && Assert
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _workflowFormService.CreateWithSpecifiedIdAsync(newId, itemToCreate));
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var itemToUpdate = await _workflowFormService.ReadAsync(id);

            // Act
            itemToUpdate.Title = Guid.NewGuid().ToGuidString();
            await _workflowFormService.UpdateAsync(id, itemToUpdate);
            var readItem = await _workflowFormService.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.Id.ShouldBeEquivalentTo(id);
            readItem.Title.ShouldBeEquivalentTo(itemToUpdate.Title);
        }

        [TestMethod]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var itemToCreate = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id, itemToCreate);
            var itemToUpdate = await _workflowFormService.ReadAsync(id);

            // Act & Assert
            itemToUpdate.Title = Guid.NewGuid().ToGuidString();
            itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _workflowFormService.UpdateAsync(id, itemToUpdate));
        }

        [TestMethod]
        public async Task Update_Given_ExistingTitle_Gives_Exception()
        {
            // Arrange
            var id1 = Guid.NewGuid().ToGuidString();
            var itemToCreate1 = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id1, itemToCreate1);
            var id2 = Guid.NewGuid().ToGuidString();
            var itemToCreate2 = new WorkflowFormCreate
            {
                CapabilityName = Guid.NewGuid().ToGuidString(),
                Title = Guid.NewGuid().ToGuidString()
            };
            await _workflowFormService.CreateWithSpecifiedIdAsync(id2, itemToCreate2);
            var itemToUpdate = await _workflowFormService.ReadAsync(id2);
            itemToUpdate.CapabilityName = itemToCreate1.CapabilityName;
            itemToUpdate.Title = itemToCreate1.Title;

            // Act & assert
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _workflowFormService.UpdateAsync(id2, itemToUpdate));
        }
    }
}
