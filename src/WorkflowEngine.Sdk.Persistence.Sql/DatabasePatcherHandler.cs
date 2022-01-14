using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.DatabasePatcher;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class DatabasePatcherHandler
    {
        /// <summary>
        /// Note! Keep this in sync with patch scripts.
        /// </summary>
        public const int DatabasePatchVersion = 6;

        private readonly string _connectionString;
        private readonly string _masterConnectionString;

        public DatabasePatcherHandler(string connectionString, string masterConnectionString)
        {
            _connectionString = connectionString;
            _masterConnectionString = masterConnectionString;
        }

        /// <summary>
        /// Static version of <see cref="PatchOrThrowAsync"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="masterConnectionString"></param>
        [Obsolete("Please use PatchOrThrowAsync(). Obsolete since 2022-01-14.")]
        public static void PatchIfNecessary(string connectionString, string masterConnectionString = null)
        {
            new DatabasePatcherHandler(connectionString, masterConnectionString)
                .PatchOrThrowAsync().Wait();
        }

        /// <summary>
        /// Patch the database or throw if the sdk level is too low.
        /// </summary>
        /// <exception cref="FulcrumAssertionFailedException"></exception>
        /// <exception cref="FulcrumBusinessRuleException"></exception>
        public Task PatchOrThrowAsync(CancellationToken cancellationToken = default)
        {
            return InternalPatchOrThrowAsync(DatabasePatchVersion, cancellationToken);
        }

        /// <summary>
        /// Patch the database or throw if the sdk level is too low.
        /// </summary>
        /// <exception cref="FulcrumAssertionFailedException"></exception>
        /// <exception cref="FulcrumBusinessRuleException"></exception>
        public async Task InternalPatchOrThrowAsync(int sdkPatchLevel, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(_connectionString);

            // Always patch (and maybe create database in dev environments)
            try
            {
                PatchIfNecessary();
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException($"Could not patch database '{connection.Database}': {e.Message}", e);
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

        private void PatchIfNecessary()
        {
            var traceLog = new StringBuilder();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var writer = new StringWriter(traceLog);
                using var traceListener = new TextWriterTraceListener(writer);
                var patcher = new Patcher(connection, GetBaseDir())
                    .WithCreateVersionTablesIfMissing(true)
                    .WithTraceListener(traceListener)
                    .WithHandleRollbacks(sincePatchNumber: 2);
                if (!string.IsNullOrWhiteSpace(_masterConnectionString))
                {
                    var masterConnection = new SqlConnection(_masterConnectionString);
                    patcher.WithMasterConnectionToCreateDatabaseIfmissingExperimental(masterConnection, "SQL_Latin1_General_CP1_CI_AS");
                }

                patcher.Execute();
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException($"[{nameof(DatabasePatcherHandler)}] Database patching failed: {traceLog}", e);
            }
        }

        public static DirectoryInfo GetBaseDir()
        {
            const string relativeUrl = @"sql-scripts";
            try
            {
                var codeBase = Assembly.GetExecutingAssembly().Location;
                if (codeBase.ToLowerInvariant().StartsWith("file:///")) codeBase = codeBase.Substring("file:///".Length);
                var binDirectory = new FileInfo(codeBase).Directory ?? new DirectoryInfo("");
                var dir = new DirectoryInfo(Path.Combine(binDirectory.FullName, relativeUrl));
                if (dir.Exists) return dir;
                return new DirectoryInfo(@$"contentFiles\any\any\{relativeUrl}"); // For unit tests
            }
            catch
            {
                return new DirectoryInfo(relativeUrl);
            }
        }
    }
}
