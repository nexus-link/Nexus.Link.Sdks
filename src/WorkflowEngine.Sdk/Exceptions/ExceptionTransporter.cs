using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    public class ExceptionTransporter : Exception
    {
        public ExceptionTransporter(Exception e) : base(null, e)
        {
        }
    }
}