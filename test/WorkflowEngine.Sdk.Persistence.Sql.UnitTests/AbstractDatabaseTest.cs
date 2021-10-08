using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Model;
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

        protected Task<WorkflowFormRecord> CreateWorkflowFormAsync(Guid id, WorkflowFormRecordCreate item, CancellationToken cancellationToken = default)
        {
            return ConfigurationTables.WorkflowForm.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        protected Task<WorkflowFormRecord> CreateStandardWorkflowFormAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = new WorkflowFormRecordCreate
            {
                CapabilityName = "SpaceExploration",
                Title = "Ios"
            };
            return CreateWorkflowFormAsync(id, item, cancellationToken);
        }

        protected Task<WorkflowFormRecord> CreateStandardWorkflowFormAsync(CancellationToken cancellationToken = default)
        {
            return CreateStandardWorkflowFormAsync(Guid.NewGuid(), cancellationToken);
        }

        protected async Task<WorkflowVersionRecord> CreateWorkflowVersionAsync(WorkflowVersionRecordCreate item, CancellationToken cancellationToken = default)
        {
            await ConfigurationTables.WorkflowVersion.CreateAsync(item, cancellationToken);
            return await ConfigurationTables.WorkflowVersion.ReadAsync(item.WorkflowFormId, item.MajorVersion, cancellationToken);
        }

        protected async Task<WorkflowVersionRecord> CreateStandardWorkflowVersionAsync(CancellationToken cancellationToken = default)
        {
            var workflowForm = await CreateStandardWorkflowFormAsync(cancellationToken);
            var item = new WorkflowVersionRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                MajorVersion = 1,
                MinorVersion = 0,
                DynamicCreate = true
            };
            await ConfigurationTables.WorkflowVersion.CreateAsync(item, cancellationToken);
            return await ConfigurationTables.WorkflowVersion.ReadAsync(item.WorkflowFormId, item.MajorVersion, cancellationToken);
        }

        protected async Task<WorkflowVersionParameterRecord> CreateWorkflowVersionParameterAsync(WorkflowVersionParameterRecordCreate item, CancellationToken cancellationToken = default)
        {
            await ConfigurationTables.WorkflowVersionParameter.CreateAsync(item, cancellationToken);
            return await ConfigurationTables.WorkflowVersionParameter.ReadAsync(item.WorkflowVersionId, item.Name, cancellationToken);
        }

        protected Task<ActivityFormRecord> CreateActivityFormAsync(Guid id, ActivityFormRecordCreate item, CancellationToken cancellationToken = default)
        {
            return ConfigurationTables.ActivityForm.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        protected async Task<ActivityFormRecord> CreateStandardActivityFormAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var workflowForm = await CreateStandardWorkflowFormAsync(cancellationToken);
            var item = new ActivityFormRecordCreate
            {
                WorkflowFormId = workflowForm.Id,
                Type = WorkflowActivityTypeEnum.Action.ToString(),
                Title = "Phobos"
            };
            return await CreateActivityFormAsync(id, item, cancellationToken);
        }
    }
}
