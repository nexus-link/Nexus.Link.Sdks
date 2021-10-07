using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    public abstract class AbstractDatabaseTest
    {
        protected const string MasterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;";
        protected const string ConnectionString = "Server=localhost;Database=workflow_engine_sdk_tests;Trusted_Connection=True;";

        protected static readonly Tenant Tenant = new Tenant("workflowenginesdk", "integrationtests");

        protected IConfigurationTables ConfigurationTables;

        static AbstractDatabaseTest()
        {
            FulcrumApplicationHelper.UnitTestSetup("Workflow engine database tests");

            DropDatabase();
            DatabasePatcherHandler.PatchIfNecessary(Tenant, ConnectionString, MasterConnectionString);
        }

        protected AbstractDatabaseTest()
        {
            ConfigurationTables = new ConfigurationTablesSql(ConnectionString);
        }

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

        protected Task<WorkflowFormRecord> CreateWorkflowForm(Guid id, WorkflowFormRecordCreate item, CancellationToken cancellationToken = default)
        {
            return ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        protected Task<WorkflowFormRecord> CreateStandardWorkflowForm(Guid id, CancellationToken cancellationToken = default)
        {
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = "SpaceExploration",
                Title = "Ios"
            };
            return CreateWorkflowForm(id, item, cancellationToken);
        }
    }
}
