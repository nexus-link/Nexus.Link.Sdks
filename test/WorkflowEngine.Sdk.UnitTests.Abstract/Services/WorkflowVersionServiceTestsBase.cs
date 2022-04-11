using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class WorkflowVersionServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IWorkflowVersionService _workflowVersionService;

        protected WorkflowVersionServiceTestsBase(IWorkflowVersionService workflowVersionService)
        {
            _workflowVersionService = workflowVersionService;
        }

        [Fact]
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
            await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var readItem = await _workflowVersionService.ReadAsync(id);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(masterId, readItem.WorkflowFormId);
            Assert.Equal(majorVersion, readItem.MajorVersion);
            Assert.Equal(itemToCreate.MinorVersion, readItem.MinorVersion);
            Assert.Equal(itemToCreate.DynamicCreate, readItem.DynamicCreate);
        }

        [Fact]
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
            await _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);

            // Act && Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowVersionService.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate));
        }

        [Fact]
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
            Assert.Equal(masterId, readItem.WorkflowFormId);
            Assert.Equal(majorVersion, readItem.MajorVersion);
            Assert.Equal(itemToCreate.MinorVersion+1, readItem.MinorVersion);
            Assert.True(readItem.DynamicCreate);
        }

        [Fact]
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
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowVersionService.UpdateAndReturnAsync(id, itemToUpdate));
        }
    }
}
