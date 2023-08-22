using System;

using Microsoft.Data.SqlClient;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support
{
    public abstract class AbstractDatabaseTest
    {
        protected static readonly Tenant Tenant = new Tenant("workflowenginesdk", "integrationtests");

        protected static object RollbackLock = new object();

        protected IConfigurationTables ConfigurationTables;
        protected IRuntimeTables RuntimeTables;

        public OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; } = null;

        protected AbstractDatabaseTest()
        {
            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = PersistenceHelper.ConnectionString,
                DefaultLockTimeSpan = TimeSpan.FromSeconds(30)
            };
            databaseOptions.DistributedLockTable = new DistributedLockTable(databaseOptions);
            ConfigurationTables = new ConfigurationTablesSql(databaseOptions);
            RuntimeTables = new RuntimeTablesSql(databaseOptions);
        }

        protected static void DropDatabase(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            using var masterConnection = new SqlConnection(PersistenceHelper.MasterConnectionString);
            masterConnection.VerifyAvailability();

            var dropCommand = masterConnection.CreateCommand();
            dropCommand.CommandText = $"IF EXISTS(SELECT 1 from sys.databases WHERE name='{connectionStringBuilder.InitialCatalog}')\n" +
                                      $"BEGIN\n" +
                                      $"   ALTER DATABASE [{connectionStringBuilder.InitialCatalog}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\n" +
                                      $"   DROP DATABASE [{connectionStringBuilder.InitialCatalog}];\n" +
                                      $"END";
            dropCommand.ExecuteNonQuery();
            masterConnection.Close();
        }
        
    }
}
