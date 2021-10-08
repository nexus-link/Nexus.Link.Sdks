using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class WorkflowInstanceTableTests : AbstractDatabaseTest
    {
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
                InitialVersion = "1.0"
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
        }

        // TODO: Test update

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
                InitialVersion = initialVersion
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateWorkflowInstanceAsync(id, item));
        }

        // TODO: expect SqlException for bad input
    }
}
