using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error.Logic;
using Shouldly;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowState.UnitTests.Services
{
    public abstract class WorkflowSemaphoreTestBase
    {
        private readonly IWorkflowSemaphoreService _service;

        protected WorkflowSemaphoreTestBase(IWorkflowSemaphoreService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateOrTakeOverOrEnqueue_01_Given_New_Gives_Id()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };

            // Act
            var id = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Assert
            id.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateOrTakeOverOrEnqueue_02_Given_ExistingAndSame_Gives_SameId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            var expectedId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Act
            var actualId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Assert
            actualId.ShouldBe(expectedId);
        }

        [Fact]
        public async Task CreateOrTakeOverOrEnqueue_03_Given_ExistingAndOtherInstanceAndLowered_Gives_SameId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = false
            };
            var expectedId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act
            var actualId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Assert
            actualId.ShouldBe(expectedId);
        }

        [Fact]
        public async Task CreateOrTakeOverOrEnqueue_04_Given_ExistingAndOtherInstanceAndRaisedAndExpired_Gives_SameId()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(1)),
                Raised = true
            };
            var expectedId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act
            var actualId = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Assert
            actualId.ShouldBe(expectedId);
        }

        [Fact]
        public async Task CreateOrTakeOverOrEnqueue_05_Given_ExistingAndOtherInstanceAndRaisedAndNotExpired_Gives_Exception()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();

            // Act and assert
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task LowerAndReturnNextWorkflowInstance_01_Given_NoQueue_Gives_Null()
        {
            // Arrange
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = Guid.NewGuid().ToGuidString(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Act
            var nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, semaphoreCreate.WorkflowInstanceId);

            // Assert
            nextInstanceId.ShouldBeNull();
        }

        [Fact]
        public async Task LowerAndReturnNextWorkflowInstance_02_Given_OneInQueue_Gives_Id()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            var expectedInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = expectedInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Act
            var nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, initialInstanceId);

            // Assert
            nextInstanceId.ShouldBe(expectedInstanceId);
        }

        [Fact]
        public async Task LowerAndReturnNextWorkflowInstance_03_Given_TwoInQueue_Gives_FirstId()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // First in queue
            var expectedInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = expectedInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            semaphoreCreate.WorkflowInstanceId = Guid.NewGuid().ToGuidString();
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Act
            var nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, initialInstanceId);

            // Assert
            nextInstanceId.ShouldBe(expectedInstanceId);
        }

        [Fact]
        public async Task LowerAndReturnNextWorkflowInstance_04_Given_FirstInQueueActive_Gives_SecondThrows()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // First in queue
            var firstInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            var secondInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Deactivate initial
            var nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, initialInstanceId);
            nextInstanceId.ShouldBe(firstInstanceId);

            // Activate first
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Try second
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;

            // Act & Assert
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();
        }

        [Fact]
        public async Task LowerAndReturnNextWorkflowInstance_05_Given_FirstInQueueDone_Gives_SecondActivated()
        {
            // Arrange
            var initialInstanceId = Guid.NewGuid().ToGuidString();
            var semaphoreCreate = new WorkflowSemaphoreCreate
            {
                WorkflowFormId = Guid.NewGuid().ToGuidString(),
                ResourceIdentifier = Guid.NewGuid().ToGuidString(),
                WorkflowInstanceId = initialInstanceId,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
                Raised = true
            };
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // First in queue
            var firstInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Second in queue
            var secondInstanceId = Guid.NewGuid().ToGuidString();
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate).ShouldThrowAsync<RequestPostponedException>();

            // Deactivate initial
            var nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, initialInstanceId);
            nextInstanceId.ShouldBe(firstInstanceId);

            // Activate first
            semaphoreCreate.WorkflowInstanceId = firstInstanceId;
            await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);

            // Deactivate first
            nextInstanceId = await _service.LowerAndReturnNextWorkflowInstanceAsync(semaphoreCreate.WorkflowFormId, semaphoreCreate.ResourceIdentifier, firstInstanceId);
            nextInstanceId.ShouldBe(secondInstanceId);

            // Try second
            semaphoreCreate.WorkflowInstanceId = secondInstanceId;

            // Act & Assert
            var id = await _service.CreateOrTakeOverOrEnqueueAsync(semaphoreCreate);
            id.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
