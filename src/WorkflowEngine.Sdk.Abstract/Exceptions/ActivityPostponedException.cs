﻿using System;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions
{
    /// <summary>
    /// Throw this if you postpone your activity for a while..
    /// Set <see cref="RequestPostponedException.TryAgainAfterMinimumTimeSpan"/> if you want help
    /// to be woken up after a while.
    /// </summary>
    public class ActivityPostponedException : RequestPostponedException
    {
        /// <summary>
        /// Throw this if you postpone your activity for a while..
        /// Set <paramref name="tryAgainAfterMinimumTimeSpan"/> if you want to be woken up after a specific time.
        /// </summary>
        public ActivityPostponedException(TimeSpan? tryAgainAfterMinimumTimeSpan) : base(tryAgainAfterMinimumTimeSpan)
        {
        }
    }
}