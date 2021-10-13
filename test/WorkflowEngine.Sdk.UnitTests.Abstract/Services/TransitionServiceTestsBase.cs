using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class TransitionServiceTestsBase
    {
        private readonly ITransitionService _service;

        protected TransitionServiceTestsBase(ITransitionService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndFindAsync()
        {
            // Arrange
            var workflowVersionId = Guid.NewGuid().ToString();
            var fromActivityVersionId = Guid.NewGuid().ToString();
            var toActivityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new TransitionCreate
            {
                WorkflowVersionId = workflowVersionId,
                FromActivityVersionId = fromActivityVersionId,
                ToActivityVersionId = toActivityVersionId
            };

            // Act
            var childId = await _service.CreateChildAsync(workflowVersionId, itemToCreate);
            var readItem = await _service.FindUniqueAsync(itemToCreate);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(childId, readItem.Id);
            Assert.Equal(itemToCreate.WorkflowVersionId, readItem.WorkflowVersionId);
            Assert.Equal(itemToCreate.FromActivityVersionId, readItem.FromActivityVersionId);
            Assert.Equal(itemToCreate.ToActivityVersionId, readItem.ToActivityVersionId);
        }

        [Fact]
        public async Task Create_Given_SameActivities_Gives_Exception()
        {
            // Arrange
            var workflowVersionId = Guid.NewGuid().ToString();
            var fromActivityVersionId = Guid.NewGuid().ToString();
            var toActivityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new TransitionCreate
            {
                WorkflowVersionId = workflowVersionId,
                FromActivityVersionId = fromActivityVersionId,
                ToActivityVersionId = toActivityVersionId
            };

            // Act
            var childId = await _service.CreateChildAsync(workflowVersionId, itemToCreate);

            // Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() => _service.CreateChildAsync(workflowVersionId, itemToCreate));
        }
    }
}
