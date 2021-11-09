using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.Configuration;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Helpers;
using Xunit;

namespace WorkflowEngine.Sdk.UnitTests.Abstract.Services
{
    public abstract class ActivityParameterServiceTestsBase<TContractException>
        where TContractException : Exception
    {
        private readonly IActivityParameterService _activityParameterService;

        protected ActivityParameterServiceTestsBase(IActivityParameterService activityParameterService)
        {
            _activityParameterService = activityParameterService;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToLowerCaseString();
            var name = "Name1";
            var itemToCreate = new ActivityParameterCreate
            {
                ActivityVersionId = masterId,
                Name = name
            };

            // Act
            await _activityParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);
            var readItem = await _activityParameterService.ReadAsync(masterId, name);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(masterId, readItem.ActivityVersionId);
            Assert.Equal(name, readItem.Name);
        }

        [Fact]
        public async Task Create_Given_SameParameter_Gives_Exception()
        {
            // Arrange
            var masterId = Guid.NewGuid().ToLowerCaseString();
            var name = "Name1";
            var itemToCreate = new ActivityParameterCreate
            {
                ActivityVersionId = masterId,
                Name = name
            };
            await _activityParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate);

            // Act && Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(() =>
                _activityParameterService.CreateWithSpecifiedIdAsync(masterId, name, itemToCreate));
        }
    }
}
