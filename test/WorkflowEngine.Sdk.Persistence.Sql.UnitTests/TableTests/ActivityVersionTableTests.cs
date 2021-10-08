using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityVersionTableTests : AbstractDatabaseTest
    {
        [Fact]
        public async Task Can_Create_Activity_Version()
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();
            var activityForm = await CreateStandardActivityFormAsync();

            var item = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                ActivityFormId = activityForm.Id,
                Position = 1
            };

            // Act
            var record = await CreateActivityVersionAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.WorkflowVersionId, record.WorkflowVersionId);
            Assert.Equal(item.ActivityFormId, record.ActivityFormId);
            Assert.Equal(item.Position, record.Position);
        }

        [Fact]
        public async Task Cant_Create_Activity_Version_With_Same_Position_On_ActivityForm_And_WorkflowVersion()
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();
            var activityForm = await CreateStandardActivityFormAsync();

            var item1 = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                ActivityFormId = activityForm.Id,
                Position = 1
            };
            var item2 = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                ActivityFormId = activityForm.Id,
                Position = 1
            };

            // Act
            await CreateActivityVersionAsync(item1);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateActivityVersionAsync(item2));
        }

        [Theory]
        [InlineData(true, false , 1)]
        [InlineData(false, true, 1)]
        [InlineData(false, false, 0)]
        [InlineData(false, false, -1)]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullWvId, bool nullFormId, int position)
        {
            // Arrange
            var workflowVersion = nullWvId ? null : await CreateStandardWorkflowVersionAsync();
            var activityForm = nullFormId ? null: await CreateStandardActivityFormAsync();

            var item = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                ActivityFormId = activityForm?.Id ?? Guid.Empty,
                Position = position
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateActivityVersionAsync(item));
        }
    }
}
