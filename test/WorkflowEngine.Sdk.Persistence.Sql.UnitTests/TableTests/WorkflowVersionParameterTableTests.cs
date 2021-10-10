using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowVersionParameterTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowVersionParameterTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Create_Workflow_Version_Parameter()
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowVersionParameterRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Name = "Moon"
            };

            // Act
            var record = await CreateWorkflowVersionParameterAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.WorkflowVersionId, record.WorkflowVersionId);
            Assert.Equal(item.Name, record.Name);
        }

        [Fact]
        public async Task Cant_Create_Workflow_Version_Parameter_With_Same_Workflow_And_Name()
        {
            // Arrange
            var workflowVersion = await CreateStandardWorkflowVersionAsync();

            var item1 = new WorkflowVersionParameterRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Name = "Moon"
            };
            var item2 = new WorkflowVersionParameterRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Name = "Moon"
            };

            // Act
            await CreateWorkflowVersionParameterAsync(item1);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateWorkflowVersionParameterAsync(item2));
        }

        [Theory]
        [InlineData(true, "Moon")]
        [InlineData(false, null)]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullVersionId, string name)
        {
            // Arrange
            var workflowVersion = nullVersionId ? null : await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowVersionParameterRecordCreate
            {
                WorkflowVersionId = workflowVersion?.Id ?? Guid.Empty,
                Name = name
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowVersionParameterAsync(item));
        }

        [Theory]
        [InlineData(null, "column does not allow nulls")]
        [InlineData("", "CK_WorkflowVersionParameter_Name_WS")]
        [InlineData(" ", "CK_WorkflowVersionParameter_Name_WS")]
        public async Task Database_Prevents_Creating_With_Bad_Input(string name, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var workflowVersion = await CreateStandardWorkflowVersionAsync();

            var item = new WorkflowVersionParameterRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Name = name
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    connection.Execute($"INSERT INTO WorkflowVersionParameter (" +
                                       $" {nameof(WorkflowVersionParameterRecord.WorkflowVersionId)}," +
                                       $" {nameof(WorkflowVersionParameterRecord.Name)})" +
                                       $" VALUES (@WorkflowVersionId, @Name)", item);
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
