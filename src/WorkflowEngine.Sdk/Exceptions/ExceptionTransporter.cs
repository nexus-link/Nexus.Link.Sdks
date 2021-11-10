using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    public class ExceptionTransporter : Exception
    {
        public ExceptionTransporter(Exception e) : base(null, e)
        {
        }
    }
}