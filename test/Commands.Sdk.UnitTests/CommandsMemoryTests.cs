using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Commands.Sdk;

namespace Commands.Sdk.UnitTests

{
    [TestClass]

    public class CommandsMemorySqlTests : CommandsTestsBase
    {
        protected override NexusCommandsOptions CommandsOptions { get; } = new NexusCommandsOptions(ServiceName, InstanceId) { UseHangfireMemoryStorage = true };

    }
}
