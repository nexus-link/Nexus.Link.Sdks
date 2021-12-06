using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowInstanceTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowInstanceTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Create_Workflow_Instance()
        {
            // Arrange
            var id = Guid.Parse("6F00C382-8B15-4F99-A4DA-C06B91ED7D2D");
            var workflowVersion = await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowInstanceRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Title = "Flying to Deimos",
                StartedAt = DateTimeOffset.Now,
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Executing.ToString()
            };

            // Act
            var record = await CreateWorkflowInstanceAsync(id, item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(id, record.Id);
            Assert.Equal(item.WorkflowVersionId, record.WorkflowVersionId);
            Assert.Equal(item.Title, record.Title);
            Assert.Equal(item.StartedAt, record.StartedAt);
            Assert.Equal(item.InitialVersion, record.InitialVersion);
            Assert.Equal(item.State, record.State);
        }

        [Theory]
        [InlineData(true, false, "Title", false, "1.0")]
        [InlineData(false, true, "Title", false, "1.0")]
        [InlineData(false, false, null, false, "1.0")]
        [InlineData(false, false, "", false, "1.0")]
        [InlineData(false, false, " ", false, "1.0")]
        [InlineData(false, false, "Title", false, null)]
        [InlineData(false, false, "Title", false, "")]
        [InlineData(false, false, "Title", false, " ")]
        [InlineData(false, false, "Title", true, "1.0")]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullId, bool nullVersion, string title, bool futureStartedAt, string initialVersion)
        {
            // Arrange
            var id = nullId ? Guid.Empty : Guid.NewGuid();
            var workflowVersion = nullVersion ? null : await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowInstanceRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                Title = title,
                StartedAt = futureStartedAt ? DateTimeOffset.Now.AddSeconds(1) : DateTimeOffset.Now.AddSeconds(-1),
                InitialVersion = initialVersion,
                State = WorkflowStateEnum.Executing.ToString()
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowInstanceAsync(id, item));
        }

        [Theory]
        [InlineData(null, "1.0", "column does not allow nulls")]
        [InlineData("", "1.0", "CK_WorkflowInstance_Title_WS")]
        [InlineData(" ", "1.0", "CK_WorkflowInstance_Title_WS")]
        [InlineData("Title", null, "column does not allow nulls")]
        [InlineData("Title", "", "CK_WorkflowInstanceInitialVersion_WS")]
        [InlineData("Title", " ", "CK_WorkflowInstanceInitialVersion_WS")]
        public async Task Database_Prevents_Creating_With_Bad_Input(string title, string initialVersion, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var workflowVersion = await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowInstanceRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                Title = title,
                InitialVersion = initialVersion,
                State = WorkflowStateEnum.Executing.ToString()
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    connection.Execute($"INSERT INTO WorkflowInstance (" +
                                       $" {nameof(WorkflowInstanceRecord.WorkflowVersionId)}," +
                                       $" {nameof(WorkflowInstanceRecord.Title)}," +
                                       $" {nameof(WorkflowInstanceRecord.InitialVersion)}," +
                                       $" {nameof(WorkflowInstanceRecord.State)})" +
                                       $" VALUES (@WorkflowVersionId, @Title, @InitialVersion, @State)", item);
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
