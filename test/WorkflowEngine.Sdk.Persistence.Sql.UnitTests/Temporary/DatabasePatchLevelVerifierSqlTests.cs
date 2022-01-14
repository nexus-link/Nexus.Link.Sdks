using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Temporary
{
    public class DatabasePatchLevelVerifierSqlTests : AbstractDatabaseTest
    {
        private readonly DatabasePatcherHandler _patchLevelVerifier;
        private readonly DatabasePatcherHandler _patchLevelVerifierForDrop;

        private const string ConnectionStringForLevelVerifier = "Server=localhost;Database=workflow-sdk-tests-flv;Trusted_Connection=True;";

        public DatabasePatchLevelVerifierSqlTests()
        {
            _patchLevelVerifier = new DatabasePatcherHandler(ConnectionString, MasterConnectionString);
            _patchLevelVerifierForDrop = new DatabasePatcherHandler(ConnectionStringForLevelVerifier, MasterConnectionString);
        }

        [Fact]
        public async Task Throws_If_Sdk_Level_Is_Lower_Than_Database_Level()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FulcrumBusinessRuleException>(async () => await _patchLevelVerifier.InternalPatchOrThrowAsync(1));
        }

        [Fact]
        public void Patches_Database_To_Sdk_Level_When_Greater_Than_Database_Level()
        {
            // Arrange
            lock (RollbackLock)
            {
                DropDatabase(ConnectionStringForLevelVerifier);
                using var connection = new SqlConnection(ConnectionStringForLevelVerifier);

                // Act
                ThreadHelper.CallAsyncFromSync(async () => await _patchLevelVerifierForDrop.PatchOrThrowAsync());
                var patchLevel = connection.QuerySingle<long>("SELECT MAX(Version) FROM DbVersion");

                // Assert
                Assert.Equal(DatabasePatcherHandler.DatabasePatchVersion, patchLevel);
            }
        }

        [Fact]
        public async Task Does_Nothing_When_Sdk_Level_Is_Equal_To_Database_Level()
        {
            await using var connection = new SqlConnection(ConnectionString);

            var patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");
            Assert.Equal(DatabasePatcherHandler.DatabasePatchVersion, patchLevel);

            await _patchLevelVerifier.PatchOrThrowAsync();
            patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");

            Assert.Equal(DatabasePatcherHandler.DatabasePatchVersion, patchLevel);
        }
    }
}
