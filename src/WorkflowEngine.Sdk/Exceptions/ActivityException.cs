using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class ActivityException : Exception
    {
        public ActivityExceptionCategoryEnum ExceptionCategory { get; }
        public string TechnicalMessage { get; }
        public string FriendlyMessage { get; }

        public ActivityException(ActivityExceptionCategoryEnum exceptionCategory, 
            string technicalMessage, string friendlyMessage)
        :base(technicalMessage)
        {
            ExceptionCategory = exceptionCategory;
            TechnicalMessage = technicalMessage;
            FriendlyMessage = friendlyMessage;
        }
    }
}