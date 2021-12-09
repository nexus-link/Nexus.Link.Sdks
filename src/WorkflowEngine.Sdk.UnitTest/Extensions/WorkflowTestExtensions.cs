using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.UnitTest.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Extensions
{
    public static class WorkflowTestExtensions
    {
        public static async Task ExecutePartiallyAsync(this IWorkflowImplementation workflow,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await workflow.ExecuteAsync(cancellationToken);
                if (workflow is WorkflowFastForward) return;
                throw new WorkflowUnitTestFailedException($"Did not expect the workflow to complete");
            }
            catch (WorkflowFastForwardBreakException)
            {
                if (workflow is WorkflowFastForward) return;
                throw new WorkflowUnitTestFailedException(
                    $"Did not expect an exception of type {nameof(WorkflowFastForwardBreakException)}.");

            }
            catch (RequestPostponedException)
            {
                if (workflow is WorkflowFastForward)
                {
                    throw new WorkflowUnitTestFailedException(
                        $"Did not expect an exception of type {nameof(RequestPostponedException)} in" +
                        $" a {nameof(WorkflowFastForward)}.{nameof(ExecutePartiallyAsync)} method.");
                }
                // This exception was expected, so just swallow it.
            }
        }
        public static async Task ExecutePartiallyAsync<T>(this IWorkflowImplementation<T> workflow,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await workflow.ExecuteAsync(cancellationToken);
                throw new WorkflowUnitTestFailedException($"Did not expect the workflow to complete");
            }
            catch (RequestPostponedException)
            {
                // This exception was expected, so just swallow it.
            }
        }
    }
}