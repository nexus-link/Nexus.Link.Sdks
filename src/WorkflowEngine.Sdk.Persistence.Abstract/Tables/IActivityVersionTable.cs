using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using WorkflowEngine.Persistence.Abstract.Entities;

namespace WorkflowEngine.Persistence.Abstract.Tables
{
    public interface IActivityVersionTable: ICreateChild<ActivityVersionRecordCreate, ActivityVersionRecord, Guid>, IUpdate<ActivityVersionRecord, Guid>, ISearch<ActivityVersionRecord, Guid>
    {
    }
}