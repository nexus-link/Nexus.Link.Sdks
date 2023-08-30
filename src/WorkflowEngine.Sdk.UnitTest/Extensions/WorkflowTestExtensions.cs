using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.UnitTest.Exceptions;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Extensions
{
    /// <summary>
    /// Extension methods for unit testing workflows.
    /// </summary>
    public static class WorkflowTestExtensions
    {
        /// <summary>
        /// Use this for unit testing when you don't expect the workflow to finish,
        /// but to be postponed by a <see cref="WorkflowFastForwardBreakException"/> exception.
        /// </summary>
        /// <exception cref="WorkflowUnitTestFailedException">
        /// Thrown if we get a <see cref="RequestPostponedException"/> in a <see cref="WorkflowFastForward"/>
        /// class or if we get <see cref="WorkflowFastForwardBreakException"/> outside a <see cref="WorkflowFastForward"/> class.
        /// </exception>
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

        /// <summary>
        /// Use this for unit testing when you don't expect the workflow to finish,
        /// but to be postponed by a <see cref="WorkflowFastForwardBreakException"/> exception.
        /// </summary>
        /// <exception cref="WorkflowUnitTestFailedException">
        /// Thrown if we get <see cref="WorkflowFastForwardBreakException"/>.
        /// </exception>
        public static async Task ExecutePartiallyAsync<T>(this IWorkflowImplementation<T> workflow,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await workflow.ExecuteAsync(cancellationToken);
                throw new WorkflowUnitTestFailedException($"Did not expect the workflow to complete");
            }
            catch (WorkflowFastForwardBreakException)
            {
                throw new WorkflowUnitTestFailedException(
                    $"Did not expect an exception of type {nameof(WorkflowFastForwardBreakException)}.");

            }
            catch (RequestPostponedException)
            {
                // This exception was expected, so just swallow it.
            }
        }
    }
}