using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Nexus.Link.DatabasePatcher;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class DatabasePatcherHandler
    {
        private static readonly object ClassLock = new();
        private static bool _isAlreadyRunning;

        public static void PatchIfNecessary(string connectionString, string masterConnectionString = null)
        {
            lock (ClassLock)
            {
                if (_isAlreadyRunning) return;
                _isAlreadyRunning = true;
            }
            var traceLog = new StringBuilder();
            try
            {
                using var connection = new SqlConnection(connectionString);
                using var writer = new StringWriter(traceLog);
                using var traceListener = new TextWriterTraceListener(writer);
                var patcher = new Patcher(connection, GetBaseDir())
                    .WithCreateVersionTablesIfMissing(true)
                    .WithTraceListener(traceListener)
                    .WithHandleRollbacks(2);
                if (!string.IsNullOrWhiteSpace(masterConnectionString))
                {
                    var masterConnection = new SqlConnection(masterConnectionString);
                    patcher.WithMasterConnectionToCreateDatabaseIfmissingExperimental(masterConnection,
                        "SQL_Latin1_General_CP1_CI_AS");
                }

                patcher.Execute();
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException(
                    $"[{nameof(DatabasePatcherHandler)}] Database patching failed: {traceLog}", e);
            }
            finally
            {
                _isAlreadyRunning = false;
            }
        }

        public static DirectoryInfo GetBaseDir()
        {
            const string relativeUrl = @"sql-scripts";
            try
            {
                var codeBase = Assembly.GetExecutingAssembly().Location;
                if (codeBase.ToLower().StartsWith("file:///")) codeBase = codeBase.Substring("file:///".Length);
                var binDirectory = new FileInfo(codeBase).Directory ?? new DirectoryInfo("");
                var dir = new DirectoryInfo(Path.Combine(binDirectory.FullName, relativeUrl));
                if (!dir.Exists) return new DirectoryInfo(@$"contentFiles\any\any\{relativeUrl}"); // For unit tests
                return dir;
            }
            catch
            {
                return new DirectoryInfo(relativeUrl);
            }
        }
    }
}
