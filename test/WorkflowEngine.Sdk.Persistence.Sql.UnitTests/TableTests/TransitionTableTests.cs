using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class TransitionTableTests : AbstractDatabaseTest
    {
        [Fact]
        public async Task Can_Create_Transition()
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();
            var activityVersionFrom = await CreateStandardActivityVersionAsync();
            var activityVersionTo = await CreateStandardActivityVersionAsync();

            var item = new TransitionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                FromActivityVersionId = activityVersionFrom.Id,
                ToActivityVersionId = activityVersionTo.Id
            };

            // Act
            var record = await CreateTransitionAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.WorkflowVersionId, record.WorkflowVersionId);
            Assert.Equal(item.FromActivityVersionId, record.FromActivityVersionId);
            Assert.Equal(item.ToActivityVersionId, record.ToActivityVersionId);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Cant_Create_Transition_With_Same_From_And_To_On_WorkflowVersion(bool nullFrom, bool nullTo)
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();
            var activityVersionFrom = nullFrom ? null : await CreateStandardActivityVersionAsync();
            var activityVersionTo = nullTo ? null : await CreateStandardActivityVersionAsync();

            var item1 = new TransitionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                FromActivityVersionId = activityVersionFrom?.Id,
                ToActivityVersionId = activityVersionTo?.Id
            };
            var item2 = new TransitionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                FromActivityVersionId = activityVersionFrom?.Id,
                ToActivityVersionId = activityVersionTo?.Id
            };

            // Act
            await CreateTransitionAsync(item1);
            
            // Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(async () => await CreateTransitionAsync(item2));
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullVersion, bool nullFrom, bool nullTo)
        {
            // Arrange
            var workflowVersion = nullVersion ? null : await CreateStandardWorkflowVersionAsync();
            var activityVersionFrom = nullFrom ? null : await CreateStandardActivityVersionAsync();
            var activityVersionTo = nullTo ? null : await CreateStandardActivityVersionAsync();

            var item = new TransitionRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                FromActivityVersionId = activityVersionFrom?.Id,
                ToActivityVersionId = activityVersionTo?.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateTransitionAsync(item));
        }
    }
}
