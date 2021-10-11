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
    public class WorkflowVersionTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowVersionTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Create_Workflow_Version()
        {
            // Arrange
            var workflowForm = await CreateStandardWorkflowFormAsync();

            var item = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                MajorVersion = 1,
                MinorVersion = 0,
                DynamicCreate = true
            };

            // Act
            var record = await CreateWorkflowVersionAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.WorkflowFormId, record.WorkflowFormId);
            Assert.Equal(item.MajorVersion, record.MajorVersion);
            Assert.Equal(item.MinorVersion, record.MinorVersion);
            Assert.Equal(item.DynamicCreate, record.DynamicCreate);
        }

        [Fact]
        public async Task Cant_Create_Workflow_Version_With_Same_Major_On_WorkflowForm()
        {
            // Arrange
            var workflowForm = await CreateStandardWorkflowFormAsync();
            var item1 = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                MajorVersion = 1,
                MinorVersion = 0,
                DynamicCreate = true
            };
            var item2 = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                MajorVersion = 1,
                MinorVersion = 1,
                DynamicCreate = true
            };

            // Act
            await CreateWorkflowVersionAsync(item1);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateWorkflowVersionAsync(item2));
        }

        [Theory]
        [InlineData(true, 1, 0)]
        [InlineData(false, 0, 0)]
        [InlineData(false, -1, 0)]
        [InlineData(false, 1, -1)]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullFormId, int majorVersion, int minorVersion)
        {
            // Arrange
            var workflowForm = nullFormId ? null : await CreateStandardWorkflowFormAsync();

            var item = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm?.Id ?? Guid.Empty,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                DynamicCreate = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowVersionAsync(item));
        }

        [Theory]
        [InlineData(null, 1, false, "column does not allow nulls")]
        [InlineData(0, 1, false, "CK_WorkflowVersion_MajorVersion")]
        [InlineData(-1, 1, false, "CK_WorkflowVersion_MajorVersion")]
        [InlineData(1, null, false, "column does not allow nulls")]
        [InlineData(1, -1, false, "CK_WorkflowVersion_MinorVersion")]
        [InlineData(1, 0, null, "column does not allow nulls")]
        public async Task Database_Prevents_Creating_With_Bad_Input(int? majorVersion, int? minorVersion, bool? dynamicCreate, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var workflowForm = await CreateStandardWorkflowFormAsync();

            var item = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm?.Id ?? Guid.Empty,
                MajorVersion = majorVersion ?? 0,
                MinorVersion = minorVersion ?? 0,
                DynamicCreate = dynamicCreate ?? true
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    var ma = majorVersion.HasValue ? "@MajorVersion" : "NULL";
                    var mi = minorVersion.HasValue ? "@MinorVersion" : "NULL";
                    var dy = dynamicCreate.HasValue ? "@DynamicCreate" : "NULL";
                    connection.Execute($"INSERT INTO WorkflowVersion (" +
                                       $" {nameof(WorkflowVersionRecord.WorkflowFormId)}," +
                                       $" {nameof(WorkflowVersionRecord.MajorVersion)}," +
                                       $" {nameof(WorkflowVersionRecord.MinorVersion)}," +
                                       $" {nameof(WorkflowVersionRecord.DynamicCreate)})" +
                                       $" VALUES (@WorkflowFormId, {ma}, {mi}, {dy})", item);
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
