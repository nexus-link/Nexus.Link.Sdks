using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using Nexus.Link.Commands.Sdk.RestClients;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using System.Collections.Generic;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
        /// Rest client to access Commands capability
        /// </summary>
        private static ICommandsClient _commandsClient;

        /// <summary>
        /// The service name that we represent
        /// </summary>
        private static string _serviceName;

        /// <summary>
        /// The id of the instance that we represent
        /// </summary>
        private static string _instanceId;

        /// <summary>
        /// The action to run when commands have been fetched from Commands service.
        /// Takes a list of commands as argument.
        /// </summary>
        private static Action<IEnumerable<NexusCommand>> _commandsCallback;

#if NETCOREAPP

        /// <summary>
        /// Adds support for the Nexus Commands capability.
        /// 
        /// Be sure to call <see cref="UseNexusCommands"/> to setup commands callback.
        /// </summary>
        public static IServiceCollection AddNexusCommands(this IServiceCollection services, NexusCommandsOptions options, ICommandsClient client)
        {
            FulcrumApplication.Setup.Validate("Code location: 07318A47-3F9E-430D-8BAF-B9F640BB793A");
            InternalContract.RequireValidated(options, nameof(options));
            InternalContract.RequireNotNull(client, nameof(client));

            _commandsClient = client;
            _serviceName = options.ServiceName;
            _instanceId = options.InstanceId;

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
        public static IApplicationBuilder UseNexusCommands(this IApplicationBuilder app, Action<IEnumerable<NexusCommand>> callback)
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
        public static void UseNexusCommands(NexusCommandsOptions options, Action<IEnumerable<NexusCommand>> callback, ICommandsClient client)
        {
            FulcrumApplication.Setup.Validate("Code location: 07318A47-3F9E-430D-8BAF-B9F640BB793A");
            InternalContract.RequireValidated(options, nameof(options));
            InternalContract.RequireNotNull(client, nameof(client));

            _commandsCallback = callback;
            _commandsClient = client;
            _serviceName = options.ServiceName;
            _instanceId = options.InstanceId;

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

        private static bool _fetchingCommands;
        private static DateTimeOffset _lastLoggedCommandFetchingError;

        /// <summary>
        /// Check with Fundamentals' Commands capability if there is anything for us.
        /// </summary>
        /// <returns></returns>
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public static async Task PollForCommands()
        {
            // One process at a time
            if (_fetchingCommands) return;
            _fetchingCommands = true;

            try
            {
                var commands = await _commandsClient.ReadAsync(_serviceName, _instanceId);
                if (commands != null)
                {
                    var nexusCommands = commands as NexusCommand[] ?? commands.ToArray();
                    if (nexusCommands.Any())
                    {
                        _commandsCallback(nexusCommands);
                    }
                }
            }
            catch (Exception e)
            {
                var now = DateTimeOffset.Now;
                if (_lastLoggedCommandFetchingError == default || _lastLoggedCommandFetchingError.AddSeconds(60) > now)
                {
                    Log.LogError($"Error fetching Commands: {e.Message}.", e);
                    _lastLoggedCommandFetchingError = now;
                }
            }
            finally
            {
                _fetchingCommands = false;
            }

        }
    }
}
