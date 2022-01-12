using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Helpers;
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
            var workflowVersionId = Guid.NewGuid().ToGuidString();
            var fromActivityVersionId = Guid.NewGuid().ToGuidString();
            var toActivityVersionId = Guid.NewGuid().ToGuidString();
            var itemToCreate = new TransitionCreate
            {
                WorkflowVersionId = workflowVersionId,
                FromActivityVersionId = fromActivityVersionId,
                ToActivityVersionId = toActivityVersionId
            };

            // Act
            var childId = await _service.CreateChildAsync(workflowVersionId, itemToCreate);
            var readItem = await _service.FindUniqueAsync(workflowVersionId, itemToCreate);

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
            var workflowVersionId = Guid.NewGuid().ToGuidString();
            var fromActivityVersionId = Guid.NewGuid().ToGuidString();
            var toActivityVersionId = Guid.NewGuid().ToGuidString();
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
