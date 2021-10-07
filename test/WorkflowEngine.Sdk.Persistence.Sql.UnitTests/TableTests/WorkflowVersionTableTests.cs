using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowVersionTableTests : AbstractDatabaseTest
    {
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
    }
}
