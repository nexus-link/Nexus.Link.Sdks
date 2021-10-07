using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Nexus.Link.DatabasePatcher;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql
{
    public class DatabasePatcherHandler
    {
        public static void PatchIfNecessary(Tenant tenant, string connectionString, string masterConnectionString = null)
        {
            var traceLog = new StringBuilder();
            try
            {
                using var connection = new SqlConnection(connectionString);
                using var writer = new StringWriter(traceLog);
                using var traceListener = new TextWriterTraceListener(writer);
                var patcher = new Patcher(connection, GetBaseDir())
                    .WithConfiguration($"{tenant.Organization}_{tenant.Environment}")
                    .WithCreateVersionTablesIfMissing(true)
                    .WithTraceListener(traceListener)
                    .WithHandleRollbacks(2);
                if (!string.IsNullOrWhiteSpace(masterConnectionString))
                {
                    var masterConnection = new SqlConnection(masterConnectionString);
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
            const string relativeUrl = @"WorkflowEngine\\sql-scripts";
            try
            {
                var codeBase = Assembly.GetExecutingAssembly().Location;
                if (codeBase.ToLower().StartsWith("file:///")) codeBase = codeBase.Substring("file:///".Length);
                var binDirectory = new FileInfo(codeBase).Directory ?? new DirectoryInfo("");
                return new DirectoryInfo(Path.Combine(binDirectory.FullName, relativeUrl));
            }
            catch
            {
                return new DirectoryInfo(relativeUrl);
            }
        }
    }
}
