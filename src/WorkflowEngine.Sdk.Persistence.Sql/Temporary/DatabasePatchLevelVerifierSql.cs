using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql.Temporary
{
    public class DatabasePatchLevelVerifierSql : IVerifyDatabasePatchLevel
    {
        private readonly string _connectionString;
        private readonly string _masterConnectionString;

        public DatabasePatchLevelVerifierSql(string connectionString, string masterConnectionString)
        {
            _connectionString = connectionString;
            _masterConnectionString = masterConnectionString;
        }

        /// <inheritdoc />
        public async Task VerifyDatabasePatchLevel(int sdkPatchLevel, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(1, sdkPatchLevel, nameof(sdkPatchLevel));

            using var connection = new SqlConnection(_connectionString);

            // Always patch (and maybe create database in dev environments)
            try
            {
                DatabasePatcherHandler.PatchIfNecessary(_connectionString, _masterConnectionString);
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException($"Could patch database '{connection.Database}': {e.Message}", e);
            }

            // Now, if the level used by the SDK version is lower than the version in database,
            // it (probably) means some other component uses a newer SDK version
            // and that we need to upgrade our SDK version before we can access the database.
            try
            {
                var patchLevel = await connection.QuerySingleAsync<long>("SELECT MAX(Version) FROM DbVersion");
                if (patchLevel > sdkPatchLevel)
                {
                    throw new FulcrumBusinessRuleException($"The database patch level used by the Workflow Engine SDK is {sdkPatchLevel}," +
                                                           $" but the version in the database is {patchLevel}. Upgrade the SDK version." +
                                                           $" (Database: '{connection.Database}')");
                }
            }
            catch (Exception e)
            {
                if (e is FulcrumBusinessRuleException) throw;
                throw new FulcrumAssertionFailedException($"Could not read patch level of database '{connection.Database}': {e.Message}", e);
            }
        }
    }
}
