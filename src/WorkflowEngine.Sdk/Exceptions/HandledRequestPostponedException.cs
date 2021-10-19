using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    internal class HandledRequestPostponedException : RequestPostponedException
    {
        public HandledRequestPostponedException(RequestPostponedException e)
        {
            AddWaitingForIds(e.WaitingForRequestIds);
        }
        public HandledRequestPostponedException(params string[] waitingForRequestIds)
        {
            AddWaitingForIds(waitingForRequestIds);
        }
    }
}