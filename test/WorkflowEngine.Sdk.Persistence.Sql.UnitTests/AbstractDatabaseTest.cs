using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.SqlServer;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Sql;

namespace WorkflowEngine.Sdk.Persistence.Sql.IntegrationTests
{
    public abstract class AbstractDatabaseTest
    {
        protected const string MasterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;";
        protected const string ConnectionString = "Server=localhost;Database=workflow-sdk-tests;Trusted_Connection=True;";

        protected static readonly Tenant Tenant = new Tenant("workflowenginesdk", "integrationtests");

        protected static object RollbackLock = new object();

        protected IConfigurationTables ConfigurationTables;
        protected IRuntimeTables RuntimeTables;

        public OnBeforeNewSqlConnectionAsync OnBeforeNewSqlConnectionAsync { get; } = null;

        static AbstractDatabaseTest()
        {
            FulcrumApplicationHelper.UnitTestSetup("Workflow engine database tests");

            DropDatabase(ConnectionString);
            DatabasePatcherHandler.PatchIfNecessary(ConnectionString, MasterConnectionString);

            using var connection = new SqlConnection(ConnectionString);
            connection.Execute("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
        }

        protected AbstractDatabaseTest()
        {
            var databaseOptions = new DatabaseOptions
            {
                ConnectionString = ConnectionString,
                DefaultLockTimeSpan = TimeSpan.FromSeconds(30)
            };
            databaseOptions.DistributedLockTable = new DistributedLockTable(databaseOptions);
            ConfigurationTables = new ConfigurationTablesSql(databaseOptions);
            RuntimeTables = new RuntimeTablesSql(databaseOptions);
        }

        protected static void DropDatabase(string connectionString)
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
            return await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), item, cancellationToken);
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
            return await ConfigurationTables.WorkflowVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), item, cancellationToken);
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
                Type = ActivityTypeEnum.Action.ToString(),
                Title = "Phobos"
            };
            return await CreateActivityFormAsync(id, item, cancellationToken);
        }

        protected async Task<ActivityFormRecord> CreateStandardActivityFormAsync(CancellationToken cancellationToken = default)
        {
            return await CreateStandardActivityFormAsync(Guid.NewGuid(), cancellationToken);
        }

        protected async Task<ActivityVersionRecord> CreateActivityVersionAsync(ActivityVersionRecordCreate item, CancellationToken cancellationToken = default)
        {
            return await ConfigurationTables.ActivityVersion.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), item, cancellationToken);
        }

        protected async Task<ActivityVersionRecord> CreateStandardActivityVersionAsync(CancellationToken cancellationToken = default)
        {
            var workflowVersion = await CreateStandardWorkflowVersionAsync(cancellationToken);
            var activityForm = await CreateStandardActivityFormAsync(cancellationToken);
            var item = new ActivityVersionRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                ActivityFormId = activityForm.Id,
                Position = 1,
                FailUrgency = ActivityFailUrgencyEnum.Stopping.ToString()
            };
            return await CreateActivityVersionAsync(item, cancellationToken);
        }

        protected async Task<ActivityVersionParameterRecord> CreateActivityVersionParameterAsync(ActivityVersionParameterRecordCreate item, CancellationToken cancellationToken = default)
        {
            await ConfigurationTables.ActivityVersionParameter.CreateAsync(item, cancellationToken);
            return await ConfigurationTables.ActivityVersionParameter.ReadAsync(item.ActivityVersionId, item.Name, cancellationToken);
        }

        protected async Task<TransitionRecord> CreateTransitionAsync(TransitionRecordCreate item, CancellationToken cancellationToken = default)
        {
            var id = await ConfigurationTables.Transition.CreateAsync(item, cancellationToken);
            return await ConfigurationTables.Transition.ReadAsync(id, cancellationToken);
        }

        protected async Task<WorkflowInstanceRecord> CreateWorkflowInstanceAsync(Guid id, WorkflowInstanceRecordCreate item, CancellationToken cancellationToken = default)
        {
            return await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        protected async Task<WorkflowInstanceRecord> CreateStandardWorkflowInstanceAsync(CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            var workflowVersion = await CreateStandardWorkflowVersionAsync(cancellationToken);
            var item = new WorkflowInstanceRecordCreate
            {
                WorkflowVersionId = workflowVersion.Id,
                Title = "Flying to Deimos",
                StartedAt = DateTimeOffset.Now,
                InitialVersion = "1.0",
                State = WorkflowStateEnum.Executing.ToString()
            };
            return await RuntimeTables.WorkflowInstance.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
        }

        protected Task<ActivityInstanceRecord> CreateActivityInstanceAsync(ActivityInstanceRecordCreate item, CancellationToken cancellationToken = default)
        {
            return RuntimeTables.ActivityInstance.CreateWithSpecifiedIdAndReturnAsync(Guid.NewGuid(), item, cancellationToken);
        }

        protected async Task<ActivityInstanceRecord> CreateStandardActivityInstanceAsync(CancellationToken cancellationToken = default)
        {
            var activityVersion = await CreateStandardActivityVersionAsync(cancellationToken);
            var workflowInstance = await CreateStandardWorkflowInstanceAsync(cancellationToken);
            var item = new ActivityInstanceRecordCreate
            {
                ActivityVersionId = activityVersion.Id,
                WorkflowInstanceId = workflowInstance.Id,
                StartedAt = DateTimeOffset.Now,
                State = ActivityStateEnum.Waiting.ToString(),
                ParentIteration = 1
            };
            return await CreateActivityInstanceAsync(item, cancellationToken);
        }
    }
}
