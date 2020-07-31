using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using System.Threading.Tasks;
using Nexus.Link.Commands.Sdk;
#if NETCOREAPP
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
#endif

namespace Commands.Sdk.UnitTests

{
    [TestClass]

    public class CommandsTests
    {

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CommandsTests));
        }

        [TestMethod]
        public async Task Todo()
        {
#if NETCOREAPP
            var host = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddNexusCommands(new NexusCommandsOptions { UseHangfireMemoryStorage = true });
                })
                .Configure(app =>
                {
                    app.UseNexusCommands(commands =>
                    {
                        Console.WriteLine(string.Join(", ", commands.Select(x => x.Command + " (" + x.SequenceNumber + ")")));
                    });
                })
                .Build();
            await host.StartAsync();

            await Task.Delay(5000);

            await host.StopAsync();
#else
            CommandsExtensions.UseNexusCommands(new NexusCommandsOptions { UseHangfireMemoryStorage = true }, commands =>
            {
                Console.WriteLine(string.Join(", ", commands.Select(x => x.Command + " (" + x.SequenceNumber + ")")));
            });

            await Task.Delay(5000);
#endif

        }
    }
}
