using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class WorkflowFailedException : Exception
    {
        public string FriendlyMessage { get; }
        public string TechnicalMessage => Message;

        public WorkflowFailedException(string technicalMessage, string friendlyMessage) : base(technicalMessage)
        {
            FriendlyMessage = friendlyMessage;
        }
    }
}