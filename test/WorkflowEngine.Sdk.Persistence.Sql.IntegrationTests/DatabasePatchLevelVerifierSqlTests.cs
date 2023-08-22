
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;
using WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{

    [Collection("workflow-sdk-tests")]
    public class DatabasePatchLevelVerifierSqlTests : AbstractDatabaseTest
    {
        private readonly DatabasePatcherHandler _patchLevelVerifierForDrop;

        private const string ConnectionStringForLevelVerifier = "Server=localhost;Database=workflow-sdk-tests-flv;Trusted_Connection=True;Encrypt=False;Encrypt=False;";

        public DatabasePatchLevelVerifierSqlTests()
        {
            _patchLevelVerifierForDrop = new DatabasePatcherHandler(ConnectionStringForLevelVerifier,
                PersistenceHelper.MasterConnectionString);
        }

        [Fact]
        public async Task Throws_If_Sdk_Level_Is_Lower_Than_Database_Level()
        {
            // Arrange
            var patchLevelVerifier = new DatabasePatcherHandler(PersistenceHelper.ConnectionString, PersistenceHelper.MasterConnectionString);

            // Act & Assert
            await Assert.ThrowsAsync<FulcrumBusinessRuleException>(async () => await patchLevelVerifier.InternalPatchOrThrowAsync(1));
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
            // Arrange
            var patchLevelVerifier = new DatabasePatcherHandler(ConnectionStringForLevelVerifier, PersistenceHelper.MasterConnectionString);
            DropDatabase(ConnectionStringForLevelVerifier);
            await patchLevelVerifier.PatchOrThrowAsync();

            await using var connection = new SqlConnection(ConnectionStringForLevelVerifier);

            var patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");
            Assert.Equal(DatabasePatcherHandler.DatabasePatchVersion, patchLevel);

            await patchLevelVerifier.PatchOrThrowAsync();
            patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");

            Assert.Equal(DatabasePatcherHandler.DatabasePatchVersion, patchLevel);
        }
    }
}
