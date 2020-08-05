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
using Nexus.Link.Libraries.Core.Logging;
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
            var host = CreateWebHost();
            await host.StartAsync();

            Assert.IsTrue(commandsAreFetched.WaitOne(TimeSpan.FromSeconds(3)));
            await host.StopAsync();
            await host.WaitForShutdownAsync();
#else
            CommandsExtensions.UseNexusCommands(CommandsOptions, VerifyStandardCommand, _clientMock.Object);
            Assert.IsTrue(commandsAreFetched.WaitOne(TimeSpan.FromSeconds(3)));
            await Task.Yield();
#endif

            _clientMock.Verify(x => x.ReadAsync(ServiceName, InstanceId), Times.AtLeast(2));
        }

        [DataTestMethod]
        // Hangfire does NOW.AddSeconds(-1) to look for start time
        [DataRow(NexusCommandsOptions.CronSecondly, 1, 3100, 3)]
        [DataRow(NexusCommandsOptions.CronEvery2Seconds, 2, 2100, 2)]
        [DataRow(NexusCommandsOptions.CronEvery2Seconds, 2, 1100, 1)]
        [DataRow(NexusCommandsOptions.CronEvery10Seconds, 10, 1500, 1)]
        [DataRow(NexusCommandsOptions.CronEvery10Seconds, 10, 4500, 1)]
        public async Task Polling_Intervals_Can_Be_Adjusted(string cronExpression, int secondMod, int timeoutMilliSeconds, int expectedPollCount)
        {
            // We want to start on the right second, so we now how many occurrences to expect
            while (DateTimeOffset.Now.Second % secondMod != 0) await Task.Delay(100);

            var clientHasBeenCalled = new ManualResetEvent(false);
            var commandCount = 0;
            _clientMock
                .Setup(x => x.ReadAsync(ServiceName, InstanceId))
                .Callback((string name, string id) =>
                {
                    if (++commandCount == expectedPollCount) clientHasBeenCalled.Set();
                    Log.LogInformation($"Client call {commandCount}, {DateTimeOffset.Now:T}.{DateTimeOffset.Now.Millisecond}");
                })
                .ReturnsAsync((IEnumerable<NexusCommand>)null)
                .Verifiable();

            CommandsOptions.PollingCronExpresson = cronExpression;

#if NETCOREAPP
            var host = CreateWebHost();
            await host.StartAsync();

            Assert.IsTrue(clientHasBeenCalled.WaitOne(TimeSpan.FromMilliseconds(timeoutMilliSeconds)) || expectedPollCount == 0);
            await host.StopAsync();
            await host.WaitForShutdownAsync();
#else
            CommandsExtensions.UseNexusCommands(CommandsOptions, VerifyStandardCommand, _clientMock.Object);
            Assert.IsTrue(clientHasBeenCalled.WaitOne(TimeSpan.FromMilliseconds(timeoutMilliSeconds)) || expectedPollCount == 0);
            await Task.Yield();
#endif

            _clientMock.Verify(x => x.ReadAsync(ServiceName, InstanceId), Times.Exactly(expectedPollCount));
        }


        [TestMethod]
        public async Task Delayed_Calls_To_Commands_Capability_Does_Not_Build_A_Queue()
        {
            const int expectedPollCount = 2;
            var commandCount = 0;
            _clientMock
                .Setup(x => x.ReadAsync(ServiceName, InstanceId))
                .Callback((string name, string id) =>
                {
                    commandCount++;
                    Log.LogInformation($"Client call {commandCount}, {DateTimeOffset.Now:T}.{DateTimeOffset.Now.Millisecond}");
                })
                .ReturnsAsync(() =>
                {
                    Thread.Sleep(2500);
                    return null;
                })
                .Verifiable();

            CommandsOptions.PollingCronExpresson = NexusCommandsOptions.CronSecondly;

#if NETCOREAPP
            var host = CreateWebHost();
            await host.StartAsync();

            await Task.Delay(5000);
            await host.StopAsync();
            await host.WaitForShutdownAsync();
#else
            CommandsExtensions.UseNexusCommands(CommandsOptions, VerifyStandardCommand, _clientMock.Object);
            await Task.Delay(5000);
            await Task.Yield();
#endif

            _clientMock.Verify(x => x.ReadAsync(ServiceName, InstanceId), Times.Exactly(expectedPollCount));
        }

#if NETCOREAPP

        private static int _createWebHostCounter;

        private IWebHost CreateWebHost()
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://*:{5000 + _createWebHostCounter++}") // We kept getting "Failed to bind to address http://127.0.0.1:5000"
                .ConfigureServices(services => { services.AddNexusCommands(CommandsOptions, _clientMock.Object); })
                .Configure(app => { app.UseNexusCommands(VerifyStandardCommand); })
                .Build();
            return host;
        }

#endif

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
