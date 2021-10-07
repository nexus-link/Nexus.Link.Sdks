using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowFormTableTests : AbstractDatabaseTest
    {
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
            var record = await CreateWorkflowForm(id, item);

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
        public async Task Cant_Create_Workflow_Form_With_Existing_Id()
        {
            // Arrange
            var id = Guid.Parse("08A03302-C861-4003-8917-7C495E205562");

            // Act
            await CreateStandardWorkflowForm(id);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateStandardWorkflowForm(id));
        }

        [Theory]
        [InlineData(null, "Title")]
        [InlineData("", "Title")]
        [InlineData(" ", "Title")]
        [InlineData("Cap name", null)]
        [InlineData("Cap name", "")]
        [InlineData("Cap name", " ")]
        public async Task Validation_Prevents_From_Creating_With_Missing_Mandatory_Columns(string capabilityName, string title)
        {
            // Arrange
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = capabilityName,
                Title = title
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowForm(Guid.NewGuid(), item));
        }

        // TODO: Override IValidated and expect SqlException for missing columns
    }
}
