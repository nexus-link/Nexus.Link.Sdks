using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityFormTable: ICreateChildWithSpecifiedId<ActivityFormRecordCreate, ActivityFormRecord, Guid>, IRead<ActivityFormRecord, Guid>, IUpdate<ActivityFormRecord, Guid>
    {
    }
}