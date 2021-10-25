using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityVersionTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ActivityVersionTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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
                FailUrgency = ActivityFailUrgencyEnum.HandleLater.ToString(),
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
            Assert.Equal(item.FailUrgency, record.FailUrgency);
        }

        [Fact]
        public async Task Can_Update_Activity_Version()
        {
            // Arrange
            var recordToUpdate = await CreateStandardActivityVersionAsync();
            recordToUpdate.Position = 2;
            recordToUpdate.FailUrgency = ActivityFailUrgencyEnum.Ignore.ToString();

            // Act
            await ConfigurationTables.ActivityVersion.UpdateAsync(recordToUpdate.Id, recordToUpdate);
            var updatedRecord = await ConfigurationTables.ActivityVersion.ReadAsync(recordToUpdate.Id);

            // Assert
            Assert.NotNull(updatedRecord);
            Assert.NotEqual(recordToUpdate.RecordCreatedAt, updatedRecord.RecordUpdatedAt);
            Assert.NotEqual(recordToUpdate.Etag, updatedRecord.Etag);
            Assert.Equal(recordToUpdate.Position, updatedRecord.Position);
            Assert.Equal(recordToUpdate.FailUrgency, updatedRecord.FailUrgency);
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
                Position = 1,
                FailUrgency = ActivityFailUrgencyEnum.Stopping.ToString()
            };
            var item2 = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                ActivityFormId = activityForm.Id,
                Position = 1,
                FailUrgency = ActivityFailUrgencyEnum.Stopping.ToString()
            };

            // Act
            await CreateActivityVersionAsync(item1);

            // Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(async () => await CreateActivityVersionAsync(item2));
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

        [Theory]
        [InlineData(null, "column does not allow nulls")]
        [InlineData(0, "CK_ActivityVersion_Position_GT0")]
        [InlineData(-1, "CK_ActivityVersion_Position_GT0")]
        public async Task Database_Prevents_Creating_With_Bad_Input(int? position, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var workflowVersion = await CreateStandardWorkflowVersionAsync();
            var activityForm = await CreateStandardActivityFormAsync();

            var item = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                ActivityFormId = activityForm?.Id ?? Guid.Empty,
                Position = position ?? 0
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    var pos = position.HasValue ? "@Position" : "NULL";
                    connection.Execute($"INSERT INTO ActivityVersion (" +
                                       $" {nameof(ActivityVersionRecord.WorkflowVersionId)}," +
                                       $" {nameof(ActivityVersionRecord.ActivityFormId)}," +
                                       $" {nameof(ActivityVersionRecord.Position)})" +
                                       $" VALUES (@WorkflowVersionId, @ActivityFormId, {pos})", item);
                }
                catch (Exception e)
                {
                    _testOutputHelper.WriteLine($"Exception: {e.Message}");
                    if (!e.Message.Contains(partOfSqlException))
                    {
                        _testOutputHelper.WriteLine($"Error: expected '{partOfSqlException}' to be part of the SQL Exception");
                        return;
                    }
                    throw;
                }
            });
        }
    }
}
