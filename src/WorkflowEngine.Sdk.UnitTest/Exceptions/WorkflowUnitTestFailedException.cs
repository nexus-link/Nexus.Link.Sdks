using System;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Exceptions
{

    public class WorkflowUnitTestFailedException : Exception
    {
        public WorkflowUnitTestFailedException(string message) : base(message)
        {}
    }
}