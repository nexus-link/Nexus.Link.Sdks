using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IActivityFormTable: ICreateChildWithSpecifiedId<ActivityFormRecordCreate, ActivityFormRecord, Guid>, IRead<ActivityFormRecord, Guid>, IUpdate<ActivityFormRecord, Guid>
    {
    }
}