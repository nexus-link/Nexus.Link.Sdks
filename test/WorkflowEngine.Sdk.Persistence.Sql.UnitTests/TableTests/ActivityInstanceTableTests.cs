using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityInstanceTableTests : AbstractDatabaseTest
    {
        [Fact]
        public async Task Can_Create_Activity_Instance()
        {
            // Arrange
            var activityVersion = await CreateStandardActivityVersionAsync();
            var workflowInstance = await CreateStandardWorkflowInstanceAsync();

            var item = new ActivityInstanceRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                WorkflowInstanceId = workflowInstance.Id,
                StartedAt = DateTimeOffset.Now,
                Iteration = 1
            };

            // Act
            var record = await CreateAcivityInstanceAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.ActivityVersionId, record.ActivityVersionId);
            Assert.Equal(item.WorkflowInstanceId, record.WorkflowInstanceId);
            Assert.Equal(item.StartedAt, record.StartedAt);
            Assert.Equal(item.Iteration, record.Iteration);
        }

        // TODO: Test update

        [Theory]
        [InlineData(true, false, false, 1)]
        [InlineData(false, true, false, 1)]
        [InlineData(false, false, true, 1)]
        [InlineData(false, false, false, 0)]
        [InlineData(false, false, false, -1)]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullVersion, bool nullInstance, bool futureStartedAt, int iteration)
        {
            // Arrange
            var activityVersion = nullVersion ? null : await CreateStandardActivityVersionAsync();
            var workflowInstance = nullInstance ? null : await CreateStandardWorkflowInstanceAsync();

            var item = new ActivityInstanceRecordCreate
            {
                ActivityVersionId = activityVersion?.Id ?? Guid.Empty,
                WorkflowInstanceId = workflowInstance?.Id ?? Guid.Empty,
                StartedAt = futureStartedAt ? DateTimeOffset.Now.AddSeconds(1) : DateTimeOffset.Now.AddSeconds(-1),
                Iteration = iteration
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateAcivityInstanceAsync(item));
        }

        // TODO: expect SqlException for bad input
    }
}
