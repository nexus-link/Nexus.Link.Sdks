using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Shouldly;
using Xunit;
#pragma warning disable CS1591

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services
{
    public abstract class WorkflowSemaphoreTestBase
    {
        private readonly IWorkflowSemaphoreService _service;
        private readonly ITestVerify _testVerify;

        protected WorkflowSemaphoreTestBase(IWorkflowSemaphoreService service, ITestVerify testVerify)
        {
            _service = service;
            _testVerify = testVerify;
        }

        [Fact]
        public async Task Raise_01_Given_New_Gives_Id()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };

            // Act
            var id = await _service.RaiseAsync(semaphoreCreate);

            // Assert
            id.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Raise_02_Given_ExistingAndSame_Gives_SameId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var expectedId = await _service.RaiseAsync(semaphoreCreate);

            // Act
            var actualId = await _service.RaiseAsync(semaphoreCreate);

            // Assert
            actualId.ShouldBe(expectedId);
        }

        [Fact]
        public async Task Raise_03_Given_ExistingAndOtherInstanceAndLowered_Gives_NewId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var initialId = await _service.RaiseAsync(semaphoreCreate);
            await _service.LowerAsync(initialId);
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act
            var actualId = await _service.RaiseAsync(semaphoreCreate);

            // Assert
            actualId.ShouldNotBeNull();
            actualId.ShouldNotBe(initialId);
        }

        [Fact]
        public async Task Raise_04_Given_ExistingAndOtherInstanceAndRaisedAndExpired_Gives_NewId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromSeconds(-1), // Expired
                Limit = 1
            };
            var initialId = await _service.RaiseAsync(semaphoreCreate);
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act
            var actualId = await _service.RaiseAsync(semaphoreCreate);

            // Assert
            actualId.ShouldNotBeNull();
            actualId.ShouldNotBe(initialId);
        }

        [Fact]
        public async Task Raise_05_Given_ExistingAndOtherInstanceAndRaisedAndNotExpired_Gives_Exception()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(60),
                Limit = 1
            };
            await _service.RaiseAsync(semaphoreCreate);

            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act and assert
            await _service.RaiseAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task Lower_01_Given_OneInQueue_Gives_NextIsActivated()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var id1 = await _service.RaiseAsync(semaphoreCreate);

            var expectedInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = expectedInstanceId;
            await _service.RaiseAsync(semaphoreCreate)
                .ShouldThrowAsync<RequestPostponedException>();

            // Act
            await _service.LowerAsync(id1);

            // Assert
            var currentWorkflowInstanceId = await _testVerify.GetRaisedWorkflowInstanceIdsAsync();
            currentWorkflowInstanceId.ShouldNotBeNull();
            currentWorkflowInstanceId.ShouldHaveSingleItem();
            currentWorkflowInstanceId.ShouldContain(expectedInstanceId);
        }

        [Fact]
        public async Task Lower_02_Given_TwoInQueue_Gives_FirstId()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var id1 = await _service.RaiseAsync(semaphoreCreate);

            // First in queue
            var expectedInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = expectedInstanceId;
            await _service.RaiseAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();
            await _service.RaiseAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Act
            await _service.LowerAsync(id1);

            // Assert
            semaphoreCreate.WorkflowInstanceId = expectedInstanceId;
            await _service.RaiseAsync(semaphoreCreate); ;
        }

        [Fact]
        public async Task Lower_04_Given_FirstInQueueActive_Gives_SecondThrows()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var id1 = await _service.RaiseAsync(semaphoreCreate);

            // First in queue
            var firstInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.RaiseAsync(semaphoreCreate)
                .ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            var secondInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;
            await _service.RaiseAsync(semaphoreCreate)
                .ShouldThrowAsync<RequestPostponedException>();

            // Deactivate initial
            await _service.LowerAsync(id1);

            // Activate first
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.RaiseAsync(semaphoreCreate);

            // Try second
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;

            // Act & Assert
            await _service.RaiseAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task Lower_05_Given_FirstInQueueDone_Gives_SecondActivated()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ParentActivityInstanceId = Guid.NewGuid().ToGuidString(),
                ExpirationTime = TimeSpan.FromMinutes(1),
                Limit = 1
            };
            var raisedHandlerId = await _service.RaiseAsync(semaphoreCreate);

            // First in queue
            var firstInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.RaiseAsync(semaphoreCreate)
                .ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            var secondInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;
            await _service.RaiseAsync(semaphoreCreate)
                .ShouldThrowAsync<RequestPostponedException>();

            // Deactivate initial
            await _service.LowerAsync(raisedHandlerId);

            // Activate first
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            raisedHandlerId = await _service.RaiseAsync(semaphoreCreate);

            // Deactivate first
            await _service.LowerAsync(raisedHandlerId);

            // Try second
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;

            // Act & Assert
            raisedHandlerId = await _service.RaiseAsync(semaphoreCreate);
            raisedHandlerId.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
