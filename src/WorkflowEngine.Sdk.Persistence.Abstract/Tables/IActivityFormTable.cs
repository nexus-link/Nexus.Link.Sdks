using System;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Tables
{
    public interface IActivityFormTable:
        ICreateWithSpecifiedIdAndReturn<ActivityFormRecordCreate, ActivityFormRecord, Guid>,
        IRead<ActivityFormRecord, Guid>,
        IUpdateAndReturn<ActivityFormRecord, Guid>,
        ISearch<ActivityFormRecord, Guid>
    {
    }
}