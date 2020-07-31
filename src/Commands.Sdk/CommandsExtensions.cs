using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;

#if NETCOREAPP
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#else
using System.Collections.Generic;
#endif

namespace Nexus.Link.Commands.Sdk
{
    public static class CommandsExtensions
    {


        private const string CronSecondly = "* * * * * *";

        private static readonly BackgroundJobServerOptions BackgroundJobServerOptions = new BackgroundJobServerOptions
        {
            SchedulePollingInterval = TimeSpan.FromSeconds(1)
        };

    /// <summary>
    /// The action to run when commands have been fetched from Commands service.
    /// Takes a list of commands as argument.
    /// </summary>
    private static Action<List<NexusCommand>> _commandsCallback;

#if NETCOREAPP

        /// <summary>
        /// Adds support for the Nexus Commands capability.
        /// 
        /// Be sure to call <see cref="UseNexusCommands"/> to setup commands callback.
        /// </summary>
        public static IServiceCollection AddNexusCommands(this IServiceCollection services, NexusCommandsOptions options)
        {
            FulcrumApplication.Setup.Validate("Code location: 07318A47-3F9E-430D-8BAF-B9F640BB793A");
            if (!options.UseHangfireMemoryStorage)
            {
                InternalContract.RequireNotNullOrWhiteSpace(options.HangfireSqlConnectionString, nameof(options.HangfireSqlConnectionString));
            }

            services.AddHangfire(configuration =>
            {
                if (options.UseHangfireMemoryStorage) configuration.UseMemoryStorage(options.MemoryStorageOptions);
                else configuration.UseSqlServerStorage(options.HangfireSqlConnectionString, options.SqlServerStorageOptions);
            });

            return services;
        }

        /// <summary>
        /// Sets upp the use of the Nexus Commands capability.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="callback">When commands are available, this callback action will be invoked the commands</param>
        /// <returns></returns>
        public static IApplicationBuilder UseNexusCommands(this IApplicationBuilder app, Action<List<NexusCommand>> callback)
        {
            _commandsCallback = callback;

            app.UseHangfireServer(BackgroundJobServerOptions);
            RecurringJob.AddOrUpdate(() => PollForCommands(), CronSecondly);

            return app;
        }


#else

        /// <summary>
        /// Adds support for the Nexus Commands capability.
        /// </summary>
        public static void UseNexusCommands(NexusCommandsOptions options, Action<List<NexusCommand>> callback)
        {
            FulcrumApplication.Setup.Validate("Code location: 07318A47-3F9E-430D-8BAF-B9F640BB793A");
            if (!options.UseHangfireMemoryStorage)
            {
                InternalContract.RequireNotNullOrWhiteSpace(options.HangfireSqlConnectionString, nameof(options.HangfireSqlConnectionString));
            }

            _commandsCallback = callback;

            var hangfireConfiguration = GlobalConfiguration.Configuration;

            hangfireConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            if (options.UseHangfireMemoryStorage)
            {
                hangfireConfiguration.UseMemoryStorage(options.MemoryStorageOptions);
            }
            else
            {
                hangfireConfiguration.UseSqlServerStorage(options.HangfireSqlConnectionString, options.SqlServerStorageOptions);
            }

            HangfireAspNet.Use(() => new List<IDisposable> { new BackgroundJobServer(BackgroundJobServerOptions) });
            RecurringJob.AddOrUpdate(() => PollForCommands(), CronSecondly);
        }
#endif

        /// <summary>
        /// Don't run parallel fetching processes
        /// </summary>
        private static bool _fetchingCommands;

        /// <summary>
        /// Check with Fundamentals' Commands capability if there is anything for us.
        /// </summary>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public static async Task PollForCommands()
        {
            if (_fetchingCommands) return;
            _fetchingCommands = true;


            // TODO: Implement Commands fetching

            await Task.Delay(100);

            Console.WriteLine("Enter");
            var commands = new List<NexusCommand>();
            if (new Random().NextDouble() > 0.2)
            {
                commands.Add(new NexusCommand { Command = "apa1", CreatedAt = DateTimeOffset.Now, Id = Guid.NewGuid().ToString(), Originator = "Groilla", SequenceNumber = new Random().Next() });
            }
            if (new Random().NextDouble() > 0.2)
            {
                commands.Add(new NexusCommand { Command = "apa2", CreatedAt = DateTimeOffset.Now, Id = Guid.NewGuid().ToString(), Originator = "Groilla", SequenceNumber = new Random().Next() });
            }

            if (commands.Any())
            {
                _commandsCallback(commands);
            }

            Console.WriteLine("Exit");

            _fetchingCommands = false;
        }

    }
}
