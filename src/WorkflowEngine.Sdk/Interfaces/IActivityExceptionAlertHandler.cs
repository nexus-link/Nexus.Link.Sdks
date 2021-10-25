﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public interface IActivityExceptionAlertHandler
    {
        public Task<bool> HandleActivityExceptionAlertAsync(ActivityExceptionAlert alert,
            CancellationToken cancellationToken = default);
    }
}