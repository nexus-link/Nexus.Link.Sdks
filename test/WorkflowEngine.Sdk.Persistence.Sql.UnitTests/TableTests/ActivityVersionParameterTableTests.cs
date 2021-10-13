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
    public class ActivityVersionParameterTableTests : AbstractDatabaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ActivityVersionParameterTableTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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
            await Assert.ThrowsAsync<FulcrumConflictException>(async () => await CreateActivityVersionParameterAsync(item2));
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

        [Theory]
        [InlineData(null, "column does not allow nulls")]
        [InlineData("", "CK_ActivityVersionParameter_Name_WS")]
        [InlineData(" ", "CK_ActivityVersionParameter_Name_WS")]
        public async Task Database_Prevents_Creating_With_Bad_Input(string name, string partOfSqlException)
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);
            var activityVersion = await CreateStandardActivityVersionAsync();

            var item = new ActivityVersionParameterRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                Name = name
            };

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    connection.Execute($"INSERT INTO ActivityVersionParameter (" +
                                       $" {nameof(ActivityVersionParameterRecord.ActivityVersionId)}," +
                                       $" {nameof(ActivityVersionParameterRecord.Name)})" +
                                       $" VALUES (@ActivityVersionId, @Name)", item);
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
