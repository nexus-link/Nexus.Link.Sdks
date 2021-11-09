using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class WorkflowFailedException : Exception
    {
        public ActivityExceptionCategoryEnum ExceptionCategory { get; }
        public string FriendlyMessage { get; }
        public string TechnicalMessage => Message;

        public WorkflowFailedException(ActivityExceptionCategoryEnum exceptionCategory, string technicalMessage, string friendlyMessage) : base(technicalMessage)
        {
            ExceptionCategory = exceptionCategory;
            FriendlyMessage = friendlyMessage;
        }
    }
}