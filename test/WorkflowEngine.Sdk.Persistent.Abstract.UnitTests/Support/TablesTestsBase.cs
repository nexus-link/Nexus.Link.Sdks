using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.ConfigurationTablesTests;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.UnitTests.Support
{
    public class TablesTestsBase
    {
        protected readonly IConfigurationTables ConfigurationTables;
        protected readonly IRuntimeTables RuntimeTables;

        protected TablesTestsBase(IConfigurationTables configurationTables, IRuntimeTables runtimeTables)
        {
            FulcrumApplicationHelper.UnitTestSetup("WorkflowEngine.Sdk.Persistence.Abstract.UnitTests");
            ConfigurationTables = configurationTables;
            RuntimeTables = runtimeTables;
            RuntimeTables.DeleteAllAsync().Wait();
            ConfigurationTables.DeleteAllAsync().Wait();
        }
    }
}