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
    public abstract class WorkflowVersionServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IWorkflowVersionService _workflowVersionService;

        protected WorkflowVersionServiceTestsBase(IWorkflowVersionService workflowVersionService)
        {
            _workflowVersionService = workflowVersionService;
        }

        [TestMethod]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var masterId = Guid.NewGuid().ToGuidString();
            var majorVersion = 1;
            var itemToCreate = new WorkflowVersionCreate
            {
                WorkflowFormId = masterId,
                MajorVersion = majorVersion,
                MinorVersion = 3,
                DynamicCreate = true
            };

            // Act
            var createdItem = await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var readItem = await _workflowVersionService.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.WorkflowFormId.ShouldBeEquivalentTo(masterId);
            readItem.MajorVersion.ShouldBeEquivalentTo(majorVersion);
            readItem.MinorVersion.ShouldBeEquivalentTo(itemToCreate.MinorVersion);
            readItem.DynamicCreate.ShouldBeEquivalentTo(itemToCreate.DynamicCreate);
        }

        [TestMethod]
        public async Task Create_Given_SameVersion_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var masterId = Guid.NewGuid().ToGuidString();
            var majorVersion = 1;
            var itemToCreate = new WorkflowVersionCreate
            {
                WorkflowFormId = masterId,
                MajorVersion = majorVersion,
                MinorVersion = 3
            };
            var createdItem = await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);

            // Act && Assert
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid().ToGuidString(), itemToCreate));
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var masterId = Guid.NewGuid().ToGuidString();
            var majorVersion = 1;
            var itemToCreate = new WorkflowVersionCreate
            {
                WorkflowFormId = masterId,
                MajorVersion = majorVersion,
                MinorVersion = 3,
                DynamicCreate = false
            };
            await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var itemToUpdate = await _workflowVersionService.ReadAsync(id);

            // Act
            itemToUpdate.MinorVersion =itemToCreate.MinorVersion+1;
            itemToUpdate.DynamicCreate = true;
            await _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate);
            var readItem = await _workflowVersionService.ReadAsync(id);

            // Assert
            readItem.ShouldNotBeNull();
            readItem.WorkflowFormId.ShouldBeEquivalentTo(masterId);
            readItem.MajorVersion.ShouldBeEquivalentTo(majorVersion);
            readItem.MinorVersion.ShouldBeEquivalentTo(itemToUpdate.MinorVersion);
            readItem.DynamicCreate.ShouldBeEquivalentTo(itemToUpdate.DynamicCreate);
        }

        [TestMethod]
        public async Task Update_Given_WrongEtag_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var masterId = Guid.NewGuid().ToGuidString();
            var majorVersion = 1;
            var itemToCreate = new WorkflowVersionCreate
            {
                WorkflowFormId = masterId,
                MajorVersion = majorVersion,
                MinorVersion = 3
            };
            await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var itemToUpdate = await _workflowVersionService.ReadAsync(id);

            // Act & Assert
            itemToUpdate.MinorVersion =itemToCreate.MinorVersion+1;
            itemToUpdate.Etag = Guid.NewGuid().ToGuidString();
            await Should.ThrowAsync<FulcrumConflictException>(() =>
                _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate));
        }

        [TestMethod]
        public async Task Update_Given_NewMajorVersion_Gives_Exception()
        {
            // Arrange
            var id = Guid.NewGuid().ToGuidString();
            var masterId = Guid.NewGuid().ToGuidString();
            var majorVersion = 1;
            var itemToCreate = new WorkflowVersionCreate
            {
                WorkflowFormId = masterId,
                MajorVersion = majorVersion,
                MinorVersion = 3
            };
            await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var itemToUpdate = await _workflowVersionService.ReadAsync(id);

            // Act & Assert
            itemToUpdate.MajorVersion = itemToCreate.MajorVersion+1;
            itemToUpdate.MinorVersion =1;
            await Should.ThrowAsync<TContractException>(() =>
                _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate));
        }
    }
}
