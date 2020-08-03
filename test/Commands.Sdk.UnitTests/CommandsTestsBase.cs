using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    public abstract class CommandsTestsBase
    {
        protected const string ServiceName = nameof(CommandsTestsBase);
        protected const string InstanceId = "abc-123";

        private const string Originator = "origami";
        private const string StandardCommand = "fold";
        
        protected abstract NexusCommandsOptions CommandsOptions { get; }

        private Mock<ICommandsClient> _clientMock;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CommandsTestsBase));
            _clientMock = new Mock<ICommandsClient>();
        }

        [TestMethod]
        public async Task Commands_Are_Fetched_And_Processed()
        {
            var commandsAreFetched = new ManualResetEvent(false);

            var mockCommands = new List<NexusCommand> { CreateCommand() };
            var i = 0;
            _clientMock
                .Setup(x => x.ReadAsync(ServiceName, InstanceId))
                .ReturnsAsync(() =>
                {
                    if (++i == 2) // Simulate second call returns a command
                    {
                        commandsAreFetched.Set();
                        return mockCommands;
                    }
                    else return null;
                })
                .Verifiable();

#if NETCOREAPP
            var host = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services => { services.AddNexusCommands(CommandsOptions, _clientMock.Object); })
                .Configure(app => { app.UseNexusCommands(VerifyStandardCommand); })
                .Build();
            await host.StartAsync();

            Assert.IsTrue(commandsAreFetched.WaitOne(TimeSpan.FromSeconds(10)));
            
            await host.StopAsync();
#else
            CommandsExtensions.UseNexusCommands(CommandsOptions, VerifyStandardCommand, _clientMock.Object);
            Assert.IsTrue(commandsAreFetched.WaitOne(TimeSpan.FromSeconds(3)));
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
