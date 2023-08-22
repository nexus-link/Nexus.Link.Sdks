using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    /// <summary>
    /// Persistence for <see cref="ActivityInstanceRecord"/>
    /// </summary>
    public interface IActivityInstanceTable: 
        ICreateWithSpecifiedIdAndReturn<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>,
        IRead<ActivityInstanceRecord, Guid>, 
        IUpdateAndReturn<ActivityInstanceRecord, Guid>
    {
        Task<IEnumerable<ActivityInstanceRecord>> SearchByWorkflowInstanceIdAsync(Guid workflowInstanceId, int limit = int.MaxValue, CancellationToken cancellationToken = default);
    }
}