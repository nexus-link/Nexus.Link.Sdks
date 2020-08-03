using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using System.Threading.Tasks;
using Moq;
using Nexus.Link.Commands.Sdk;
using Nexus.Link.Commands.Sdk.RestClients;

#if NETCOREAPP
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
#endif

namespace Commands.Sdk.UnitTests

{
    [TestClass]

    public class CommandsTests
    {

        private const string ServiceName = nameof(CommandsTests);
        private const string InstanceId = "abc-123";

        private const string Originator = "origami";
        private const string StandardCommand = "fold";

        private static readonly NexusCommandsOptions NexusCommandsOptions = new NexusCommandsOptions(ServiceName, InstanceId) { UseHangfireMemoryStorage = true };

        private Mock<ICommandsClient> _clientMock;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CommandsTests));
        }

        [TestInitialize]
        public void Initialize()
        {
            _clientMock = new Mock<ICommandsClient>();
        }

        [TestMethod]
        public async Task Commands_Are_Fetched_And_Processed()
        {
            var mockCommands = new List<NexusCommand> { CreateCommand() };
            var i = 0;
            _clientMock
                .Setup(x => x.ReadAsync(ServiceName, InstanceId))
                .ReturnsAsync(() =>
                {
                    if (++i == 2) return mockCommands;
                    else return null;
                })
                .Verifiable();

#if NETCOREAPP
            var host = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services => { services.AddNexusCommands(NexusCommandsOptions, _clientMock.Object); })
                .Configure(app => { app.UseNexusCommands(VerifyStandardCommand); })
                .Build();
            await host.StartAsync();
            await Task.Delay(3000);
            await host.StopAsync();
#else
            CommandsExtensions.UseNexusCommands(NexusCommandsOptions, VerifyStandardCommand, _clientMock.Object);
            await Task.Delay(3000);
#endif

            _clientMock.Verify(x => x.ReadAsync(ServiceName, InstanceId), Times.AtLeast(2));
        }

        private static void VerifyStandardCommand(IEnumerable<NexusCommand> commands)
        {
            var command = commands.FirstOrDefault();
            Assert.IsNotNull(command, "There should be one command in the list");
            Console.WriteLine($"Command: {command}");
            Assert.AreEqual(Originator, command.Originator);
            Assert.AreEqual(StandardCommand, command.Command);
            Assert.IsNotNull(command.Id);
            Assert.IsTrue(command.SequenceNumber > 0);
            Assert.IsTrue(command.CreatedAt != default);
        }

        private static int _sequence;

        private static NexusCommand CreateCommand(string command = StandardCommand)
        {
            return new NexusCommand
            {
                Command = command,
                CreatedAt = DateTimeOffset.Now,
                Id = Guid.NewGuid().ToString(),
                Originator = Originator,
                SequenceNumber = ++_sequence
            };
        }
    }
}
