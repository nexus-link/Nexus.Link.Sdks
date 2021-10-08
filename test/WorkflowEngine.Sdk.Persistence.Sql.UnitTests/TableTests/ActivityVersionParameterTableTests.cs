using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.TableTests
{
    public class ActivityVersionParameterTableTests : AbstractDatabaseTest
    {
        [Fact]
        public async Task Can_Create_Activity_Version_Parameter()
        {
            // Arrange
            var activityVersion = await CreateStandardActivityVersionAsync();

            var item = new ActivityVersionParameterRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                Name = "Moon"
            };

            // Act
            var record = await CreateActivityVersionParameterAsync(item);

            // Assert
            Assert.NotNull(record);
            Assert.NotEqual(default, record.RecordCreatedAt);
            Assert.NotEqual(default, record.RecordUpdatedAt);
            Assert.NotNull(record.RecordVersion);
            Assert.NotNull(record.Etag);
            Assert.NotEqual(default, record.Id);
            Assert.Equal(item.ActivityVersionId, record.ActivityVersionId);
            Assert.Equal(item.Name, record.Name);
        }

        [Fact]
        public async Task Cant_Create_Activity_Version_Parameter_With_Same_Workflow_And_Name()
        {
            // Arrange
            var activityVersion = await CreateStandardActivityVersionAsync();

            var item1 = new ActivityVersionParameterRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                Name = "Moon"
            };
            var item2 = new ActivityVersionParameterRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                Name = "Moon"
            };

            // Act
            await CreateActivityVersionParameterAsync(item1);

            // Assert
            await Assert.ThrowsAsync<SqlException>(async () => await CreateActivityVersionParameterAsync(item2));
        }

        [Theory]
        [InlineData(true, "Moon")]
        [InlineData(false, null)]
        [InlineData(false, "")]
        [InlineData(false, " ")]
        public async Task Validation_Prevents_Creating_With_Bad_Input(bool nullVersionId, string name)
        {
            // Arrange
            var activityVersion = nullVersionId ? null : await CreateStandardActivityVersionAsync();

            var item = new ActivityVersionParameterRecordCreate
            {
                ActivityVersionId = activityVersion?.Id ?? Guid.Empty,
                Name = name
            };

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumContractException>(async () => await CreateActivityVersionParameterAsync(item));
        }
    }
}
