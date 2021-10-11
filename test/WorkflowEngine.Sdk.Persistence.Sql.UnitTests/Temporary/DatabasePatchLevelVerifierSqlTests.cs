using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Temporary;
using Nexus.Link.WorkflowEngine.Sdk.Temporary;
using Xunit;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Temporary
{
    public class DatabasePatchLevelVerifierSqlTests : AbstractDatabaseTest
    {
        private readonly IVerifyDatabasePatchLevel _patchLevelVerifier;
        private readonly IVerifyDatabasePatchLevel _patchLevelVerifierForDrop;

        private const string ConnectionStringForLevelVerifier = "Server=localhost;Database=workflow-sdk-tests-flv;Trusted_Connection=True;";

        public DatabasePatchLevelVerifierSqlTests()
        {
            _patchLevelVerifier = new DatabasePatchLevelVerifierSql(ConnectionString, MasterConnectionString);
            _patchLevelVerifierForDrop = new DatabasePatchLevelVerifierSql(ConnectionStringForLevelVerifier, MasterConnectionString);
        }

        [Fact]
        public async Task Throws_If_Sdk_Level_Is_Lower_Than_Database_Level()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FulcrumBusinessRuleException>(async () => await _patchLevelVerifier.VerifyDatabasePatchLevel(Tenant, 1));
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
                ThreadHelper.CallAsyncFromSync(async () => await _patchLevelVerifierForDrop.VerifyDatabasePatchLevel(Tenant, DatabasePatchLevel.Version));
                var patchLevel = connection.QuerySingle<long>("SELECT MAX(Version) FROM DbVersion");

                // Assert
                Assert.Equal(DatabasePatchLevel.Version, patchLevel);
            }
        }

        [Fact]
        public async Task Does_Nothing_When_Sdk_Level_Is_Equal_To_Database_Level()
        {
            await using var connection = new SqlConnection(ConnectionString);

            var patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");
            Assert.Equal(DatabasePatchLevel.Version, patchLevel);

            await _patchLevelVerifier.VerifyDatabasePatchLevel(Tenant, DatabasePatchLevel.Version);
            patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");

            Assert.Equal(DatabasePatchLevel.Version, patchLevel);
        }
    }
}
