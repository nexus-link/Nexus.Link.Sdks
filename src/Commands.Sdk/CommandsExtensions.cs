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
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Nexus.Link.Commands.Sdk
{
    public static class CommandsExtensions
    {
        private static readonly BackgroundJobServerOptions BackgroundJobServerOptions = new BackgroundJobServerOptions
        {
            SchedulePollingInterval = TimeSpan.FromSeconds(1),
            WorkerCount = 1
        };

        private static ICommandsClient _commandsClient;
        internal static NexusCommandsOptions _options;
        private static Action<IEnumerable<NexusCommand>> _commandsCallback;

#if NETCOREAPP

        /// <summary>
        /// Adds support for the Nexus Commands capability.
        /// 
        /// Be sure to call <see cref="UseNexusCommands"/> to setup commands callback.
        /// </summary>
        public static IServiceCollection AddNexusCommands(this IServiceCollection services, NexusCommandsOptions options, ICommandsClient client)
        {
            FulcrumApplication.Setup.Validate("Code location: A34467CC-0E3D-48B0-B8C4-65C4AC343FE1");
            InternalContract.RequireValidated(options, nameof(options));
            InternalContract.RequireNotNull(client, nameof(client));

            _commandsClient = client;
            _options = options;

            services.AddHangfire(configuration =>
            {
                configuration.UseLogProvider(new FulcrumHangfireLogProvider());
                if (options.UseHangfireMemoryStorage) configuration.UseMemoryStorage(options.MemoryStorageOptions);
                else configuration.UseSqlServerStorage(options.HangfireSqlConnectionString, options.SqlServerStorageOptions);
            });

            return services;
        }

        /// <summary>
        /// Setup the use of the Nexus Commands capability.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="callback">When commands are available, this callback action will be invoked the commands</param>
        /// <returns></returns>
        public static IApplicationBuilder UseNexusCommands(this IApplicationBuilder app, Action<IEnumerable<NexusCommand>> callback)
        {
            _commandsCallback = callback;

            app.UseHangfireServer(BackgroundJobServerOptions);
            SetupRecurringJob();

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
            _options = options;

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
            SetupRecurringJob();
        }
#endif

        private static void SetupRecurringJob()
        {
            const string jobId = nameof(PollForCommands);
            var manager = new RecurringJobManager();
            manager.RemoveIfExists(jobId);
            manager.AddOrUpdate(jobId, () => PollForCommands(), _options.PollingCronExpresson);
        }

        private static bool _fetchingCommands;
        private static DateTimeOffset _lastLoggedCommandFetchingError;

        /// <summary>
        /// Check with Fundamentals' Commands capability if there is anything for us.
        /// </summary>
        /// <remarks>
        /// Not intended for outside usage, but Hangfire requires a public function.
        /// </remarks>
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        [OneSecondExpirationTime]
        public static async Task PollForCommands()
        {
            // One process at a time
            if (_fetchingCommands) return;
            _fetchingCommands = true;

            try
            {
                var commands = await _commandsClient.ReadAsync(_options.ServiceName, _options.InstanceId);
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

    internal class OneSecondExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromSeconds(1);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromSeconds(1);
        }
    }

    internal class FulcrumHangfireLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new FulcrumHangfireLogger();
        }
    }

    internal class FulcrumHangfireLogger : ILog
    {
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            var message = messageFunc?.Invoke();
            switch (logLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Trace:
                    if (FulcrumApplication.Setup.LogSeverityLevelThreshold > LogSeverityLevel.Verbose) return false;
                    Libraries.Core.Logging.Log.LogVerbose(message, exception);
                    break;
                case LogLevel.Info:
                    if (FulcrumApplication.Setup.LogSeverityLevelThreshold > LogSeverityLevel.Information) return false;
                    Libraries.Core.Logging.Log.LogInformation(message, exception);
                    break;
                case LogLevel.Warn:
                    if (FulcrumApplication.Setup.LogSeverityLevelThreshold > LogSeverityLevel.Warning) return false;
                    Libraries.Core.Logging.Log.LogWarning(message, exception);
                    break;
                case LogLevel.Error:
                    if (FulcrumApplication.Setup.LogSeverityLevelThreshold > LogSeverityLevel.Error) return false;
                    Libraries.Core.Logging.Log.LogError(message, exception);
                    break;
                case LogLevel.Fatal:
                    if (FulcrumApplication.Setup.LogSeverityLevelThreshold > LogSeverityLevel.Critical) return false;
                    Libraries.Core.Logging.Log.LogCritical(message, exception);
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
