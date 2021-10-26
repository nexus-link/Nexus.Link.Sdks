using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityInstanceTable: ICreateAndReturn<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>, IRead<ActivityInstanceRecord, Guid>, IUpdateAndReturn<ActivityInstanceRecord, Guid>, ISearch<ActivityInstanceRecord, Guid>
    {
    }
}