using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityVersionTable: ICreate<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IRead<ActivityVersionRecord, Guid>, IUpdate<ActivityVersionRecord, Guid>, ISearch<ActivityVersionRecord, Guid>
    {
    }
}