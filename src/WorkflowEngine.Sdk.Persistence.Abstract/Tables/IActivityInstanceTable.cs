using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityInstanceTable: ICreate<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>, IUpdate<ActivityInstanceRecord, Guid>, ISearch<ActivityInstanceRecord, Guid>
    {
    }
}