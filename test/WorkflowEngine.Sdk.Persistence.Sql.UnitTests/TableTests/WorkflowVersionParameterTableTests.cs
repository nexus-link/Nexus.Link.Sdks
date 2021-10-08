using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowVersionParameterTableTests : AbstractDatabaseTest
    {
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

        // TODO: Test update

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

        // TODO: expect SqlException for bad input
    }
}
