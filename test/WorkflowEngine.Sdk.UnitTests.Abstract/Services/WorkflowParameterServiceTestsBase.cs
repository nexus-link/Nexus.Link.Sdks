using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class WorkflowParameterServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IWorkflowParameterService _workflowParameterService;

        protected WorkflowParameterServiceTestsBase(IWorkflowParameterService workflowParameterService)
        {
            _workflowParameterService = workflowParameterService;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToString();
            var name = "Name1";
            var itemToCreate = new WorkflowParameterCreate
            {
                WorkflowVersionId = masterId,
                Name = name
            };

            // Act
            await _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);
            var readItem = await _workflowParameterService.ReadAsync(masterId, name);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(masterId, readItem.WorkflowVersionId);
            Assert.Equal(name, readItem.Name);
        }

        [Fact]
        public async Task Create_Given_SameParameter_Gives_Exception()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToString();
            var name = "Name1";
            var itemToCreate = new WorkflowParameterCreate
            {
                WorkflowVersionId = masterId,
                Name = name
            };
            await _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);

            // Act && Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _workflowParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate));
        }
    }
}
