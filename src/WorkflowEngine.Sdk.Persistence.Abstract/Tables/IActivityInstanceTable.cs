using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IActivityInstanceTable: ICreate<ActivityInstanceRecordCreate, ActivityInstanceRecord, Guid>, IUpdate<ActivityInstanceRecord, Guid>, ISearch<ActivityInstanceRecord, Guid>
    {
    }
}