using System;
using System.Data.SqlClient;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.SqlServer.Logic;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    public abstract class AbstractDatabaseTest : IDisposable
    {
        protected const string MasterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;";
        protected const string ConnectionString = "Server=localhost;Database=workflow_engine_sdk_tests;Trusted_Connection=True;";

        protected static readonly Tenant Tenant = new Tenant("workflowenginesdk", "integrationtests");

        protected static void DropDatabase()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(ConnectionString);
            using var masterConnection = new SqlConnection(MasterConnectionString);
            masterConnection.VerifyAvailability();

            var dropCommand = masterConnection.CreateCommand();
            dropCommand.CommandText = $"IF EXISTS(SELECT 1 from sys.databases WHERE name='{connectionStringBuilder.InitialCatalog}')\n" +
                                      $"BEGIN\n" +
                                      $"   ALTER DATABASE {connectionStringBuilder.InitialCatalog} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\n" +
                                      $"   DROP DATABASE {connectionStringBuilder.InitialCatalog};\n" +
                                      $"END";
            dropCommand.ExecuteNonQuery();
            masterConnection.Close();
        }

        public void Dispose()
        {
            DropDatabase();
        }
    }
}
