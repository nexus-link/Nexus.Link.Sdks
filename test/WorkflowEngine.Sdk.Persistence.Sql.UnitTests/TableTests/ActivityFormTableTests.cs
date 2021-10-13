using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityFormTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ActivityFormTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Create_Activity_Form_With_Specified_Id()
        {
            // Arrange
            var workflowForm = await CreateStandardWorkflowFormAsync();
            var id = Guid.NewGuid();
            var item = new ActivityFormRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                Type = WorkflowActivityTypeEnum.Action.ToString(),
                Title = "Phobos"
            };

            // Act
            var record = await CreateActivityFormAsync(id, item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.Equal(id, record.Id);
            Assert.Equal(item.WorkflowFormId, record.WorkflowFormId);
            Assert.Equal(item.Type, record.Type);
            Assert.Equal(item.Title, record.Title);
        }

        [Fact]
        public async Task Can_Update_Activity_Form()
        {
            // Arrange
            var createdRecord = await CreateStandardActivityFormAsync();
            createdRecord.Type = WorkflowActivityTypeEnum.ForEachParallel.ToString();
            createdRecord.Title = "Phobos of Mars";

            // Act
            await ConfigurationTables.ActivityForm.UpdateAsync(createdRecord.Id, createdRecord);
            var updatedRecord = await ConfigurationTables.ActivityForm.ReadAsync(createdRecord.Id);

            // Assert
            Assert.NotNull(updatedRecord);
            Assert.NotEqual(createdRecord.RecordCreatedAt, updatedRecord.RecordUpdatedAt);
            Assert.NotEqual(createdRecord.Etag, updatedRecord.Etag);
            Assert.Equal(createdRecord.Type, updatedRecord.Type);
            Assert.Equal(createdRecord.Title, updatedRecord.Title);
        }

        [Fact]
        public async Task Cant_Create_Activity_Form_With_Existing_Id()
        {
            // Arrange
            var id = Guid.Parse("08A03302-C861-4003-8917-7C495E205562");

            // Act
            await CreateStandardActivityFormAsync(id);

            // Assert
            await Assert.ThrowsAsync<FulcrumConflictException>(async () => await CreateStandardActivityFormAsync(id));
        }

        [Theory]
        [InlineData(true, "Action", "Title")]
        [InlineData(false, null, "Title")]
        [InlineData(false, "", "Title")]
        [InlineData(false, " ", "Title")]
        [InlineData(false, "Action", null)]
        [InlineData(false, "Action", "")]
        [InlineData(false, "Action", " ")]
        public async Task Validation_Prevents_Creating_With_Missing_Mandatory_Columns(bool nullFormId, string type, string title)
        {
            // Arrange
            var workflowForm = nullFormId ? null : await CreateStandardWorkflowFormAsync();
            var item = new ActivityFormRecordCreate
            {
                WorkflowFormId = workflowForm?.Id ?? Guid.Empty,
                Type = type,
                Title = title
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateActivityFormAsync(Guid.NewGuid(), item));
        }

        [Theory]
        [InlineData(null, "Title", "column does not allow nulls")]
        [InlineData("", "Title", "CK_ActivityForm_Type_WS")]
        [InlineData(" ", "Title", "CK_ActivityForm_Type_WS")]
        [InlineData("Type", null, "column does not allow nulls")]
        [InlineData("Type", "", "CK_ActivityForm_Title_WS")]
        [InlineData("Type", " ", "CK_ActivityForm_Title_WS")]
        public async Task Database_Prevents_Creating_With_Bad_Input(string type, string title, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var workflowForm = await CreateStandardWorkflowFormAsync();
            var item = new ActivityFormRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                Type = type,
                Title = title
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    connection.Execute($"INSERT INTO ActivityForm (" +
                                       $" {nameof(ActivityFormRecord.WorkflowFormId)}," +
                                       $" {nameof(ActivityFormRecord.Type)}," +
                                       $" {nameof(ActivityFormRecord.Title)})" +
                                       $" VALUES (@WorkflowFormId, @Type, @Title)", item);
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
