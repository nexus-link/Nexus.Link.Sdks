using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    /// <summary>
    /// Help class to setup your application
    /// </summary>
    public static class WorkflowStatic
    {
        /// <summary>
        /// The context value provider that will be used all over the application.
        /// </summary>
        public static WorkflowContext Context{ get; } = new WorkflowContext(new AsyncLocalContextValueProvider());

    }
}
