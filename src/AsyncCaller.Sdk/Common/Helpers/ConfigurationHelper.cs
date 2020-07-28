﻿using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.AsyncCaller.Sdk.Common.Helpers
{
    public static class ConfigurationHelper
    {
        public static TimeSpan GetDefaultDeadlineTimeSpanAsync(ILeverConfiguration config)
        {
            TimeSpan timeSpan;
            try
            {
                var defaultDeadlineTimespanInSeconds = config.Value<double?>("DefaultDeadlineTimeSpanInSeconds");
                if (defaultDeadlineTimespanInSeconds != null && defaultDeadlineTimespanInSeconds.Value > 0)
                {
                    timeSpan = TimeSpan.FromSeconds(defaultDeadlineTimespanInSeconds.Value);
                }
                else
                {
                    if (!FulcrumApplication.IsInProductionOrProductionSimulation)
                    {
                        Log.LogWarning("Missing configuration for DefaultDeadlineTimeSpanInSeconds. The fallback plan is to use a deadline time span of 48 hours.");
                    }

                    timeSpan = TimeSpan.FromHours(48);
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Could not get the default deadline time for new messages. The fallback plan is to use a deadline time span of 7 days. Exception message: {e.Message}");
                timeSpan = TimeSpan.FromDays(7);
            }

            return timeSpan;
        }
    }
}
