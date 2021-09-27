using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityVersionTable: ICreateChild<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IUpdate<ActivityVersionRecord, Guid>, ISearch<ActivityVersionRecord, Guid>
    {
    }
}