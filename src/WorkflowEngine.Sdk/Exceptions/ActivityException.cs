using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class ActivityException : Exception
    {
        public ActivityExceptionCategoryEnum CategoryEnum { get; }
        public string TechnicalMessage { get; }
        public string FriendlyMessage { get; }

        public ActivityException(ActivityExceptionCategoryEnum categoryEnum, 
            string technicalMessage, string friendlyMessage)
        :base(technicalMessage)
        {
            CategoryEnum = categoryEnum;
            TechnicalMessage = technicalMessage;
            FriendlyMessage = friendlyMessage;
        }
    }
}