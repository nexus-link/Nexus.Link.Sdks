
using Microsoft.Data.SqlClient;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests.Support
{
    internal class PersistenceHelper
    {
        public const string MasterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;Encrypt=False;";
        public const string ConnectionString = "Server=localhost;Database=workflow-sdk-tests;Trusted_Connection=True;Encrypt=False;";

        public static IConfigurationTables ConfigurationTables => new ConfigurationTablesSql(DefaultDatabaseOptions);
        public static IRuntimeTables RuntimeTables => new RuntimeTablesSql(DefaultDatabaseOptions);

        public static object RollbackLock = new object();

        static PersistenceHelper()
        {
            DropDatabase(ConnectionString);
            var handler = new DatabasePatcherHandler(ConnectionString, MasterConnectionString);
            handler.PatchOrThrowAsync();
        }

        public static IDatabaseOptions DefaultDatabaseOptions
        {
            get
            {
                var options = new DatabaseOptions
                {
                    ConnectionString = ConnectionString
                };
                options.DistributedLockTable = new DistributedLockTable(options);

                return options;
            }
        }


        public static void DropDatabase(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            using var masterConnection = new SqlConnection(MasterConnectionString);
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
