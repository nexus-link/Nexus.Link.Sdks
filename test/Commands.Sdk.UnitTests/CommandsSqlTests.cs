using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Commands.Sdk;
using Nexus.Link.DatabasePatcher;
#if NETCOREAPP

#endif

namespace Commands.Sdk.UnitTests

{
    [TestClass]

    public class CommandsSqlTests : CommandsTestsBase
    {

        private const string MasterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;";
        private const string ConnectionString = "Server=localhost;Database=hangfire;Trusted_Connection=True;";


        protected override NexusCommandsOptions CommandsOptions { get; } = new NexusCommandsOptions(ServiceName, InstanceId) {HangfireSqlConnectionString = ConnectionString};

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Create database if missing
            new Patcher(new SqlConnection(ConnectionString), new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}\\scripts"))
                .WithMasterConnectionToCreateDatabaseIfmissingExperimental(new SqlConnection(MasterConnectionString))
                .WithCreateVersionTablesIfMissing(true)
                .Execute();
        }
    }
}
