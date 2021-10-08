using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowFormTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public WorkflowFormTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Create_Workflow_Form_With_Specified_Id()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = "SpaceExploration",
                Title = "Phobos"
            };

            // Act
            var record = await CreateWorkflowFormAsync(id, item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.Equal(id, record.Id);
            Assert.Equal(item.CapabilityName, record.CapabilityName);
            Assert.Equal(item.Title, record.Title);
        }

        [Fact]
        public async Task Can_Update_Workflow_Form()
        {
            // Arrange
            var createdRecord = await CreateStandardWorkflowFormAsync();
            createdRecord.CapabilityName = "New cap";
            createdRecord.Title = "New title";

            // Act
            await ConfigurationTables.WorkflowForm.UpdateAsync(createdRecord.Id, createdRecord);
            var updatedRecord = await ConfigurationTables.WorkflowForm.ReadAsync(createdRecord.Id);

            // Assert
            Assert.NotNull(updatedRecord);
            Assert.NotEqual(createdRecord.RecordCreatedAt, updatedRecord.RecordUpdatedAt);
            Assert.NotEqual(createdRecord.Etag, updatedRecord.Etag);
            Assert.Equal(createdRecord.CapabilityName, updatedRecord.CapabilityName);
            Assert.Equal(createdRecord.Title, updatedRecord.Title);
        }

        [Fact]
        public async Task Cant_Create_Workflow_Form_With_Existing_Id()
        {
            // Arrange
            var id = Guid.Parse("08A03302-C861-4003-8917-7C495E205562");

            // Act
            await CreateStandardWorkflowFormAsync(id);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateStandardWorkflowFormAsync(id));
        }

        [Theory]
        [MemberData(nameof(BadInput))]
        public async Task Validation_Prevents_Creating_With_Bad_Input(string capabilityName, string title, string _)
        {
            // Arrange
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = capabilityName,
                Title = title
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowFormAsync(Guid.NewGuid(), item));
        }

        [Theory]
        [MemberData(nameof(BadInput))]
        public async Task Database_Prevents_Creating_With_Bad_Input(string capabilityName, string title, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = capabilityName,
                Title = title
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    connection.Execute($"INSERT INTO WorkflowForm ({nameof(WorkflowFormRecord.CapabilityName)}, {nameof(WorkflowFormRecord.Title)})" +
                                       $" VALUES (@CapabilityName, @Title)", item);
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

        public static IEnumerable<object[]> BadInput()
        {
            yield return new object[] { null, "Title", "column does not allow nulls" };
            yield return new object[] { "", "Title", "CK_WorkflowForm_CapabilityName_WS" };
            yield return new object[] { " ", "Title", "CK_WorkflowForm_CapabilityName_WS" };
            yield return new object[] { "Cap name", null, "column does not allow nulls" };
            yield return new object[] { "Cap name", "", "CK_WorkflowForm_Title_WS" };
            yield return new object[] { "Cap name", " ", "CK_WorkflowForm_Title_WS" };
        }
    }
}
