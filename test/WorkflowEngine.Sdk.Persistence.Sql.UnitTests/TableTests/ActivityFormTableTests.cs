using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityFormTableTests : AbstractDatabaseTest
    {
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
        public async Task Cant_Create_Activity_Form_With_Existing_Id()
        {
            // Arrange
            var id = Guid.Parse("08A03302-C861-4003-8917-7C495E205562");

            // Act
            await CreateStandardActivityFormAsync(id);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateStandardActivityFormAsync(id));
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

        // TODO: Override IValidated and expect SqlException for missing columns
    }
}
