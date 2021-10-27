using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    /// <summary>
    /// Help class to setup your application
    /// </summary>
    public static class AsyncWorkflowStatic
    {
        /// <summary>
        /// The context value provider that will be used all over the application.
        /// </summary>
        public static AsyncWorkflowContext Context{ get; } = new AsyncWorkflowContext(new AsyncLocalContextValueProvider());

    }
}
