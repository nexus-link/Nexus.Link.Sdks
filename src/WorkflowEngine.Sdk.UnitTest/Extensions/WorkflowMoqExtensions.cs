using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.UnitTest.Extensions;

public static class WorkflowMoqExtensions
{
    public static IReturnsResult<TMocked> BreakWorkflowAsync<TMocked, TReturn>(this ISetup<TMocked, Task<TReturn>> setup) where TMocked : class
    {
        return setup.ThrowsAsync(new RequestPostponedException());
    }

    public static IReturnsResult<TMocked> BreakWorkflowAsync<TMocked>(this ISetup<TMocked, Task> setup) where TMocked : class
    {
        return setup.ThrowsAsync(new RequestPostponedException());
    }

    public static IThrowsResult BreakWorkflow<TMocked, TReturn>(this ISetup<TMocked, TReturn> setup) where TMocked : class
    {
        return setup.Throws(new RequestPostponedException());
    }

    public static IThrowsResult BreakWorkflow<TMocked>(this ISetup<TMocked> setup) where TMocked : class
    {
        return setup.Throws(new RequestPostponedException());
    }
}