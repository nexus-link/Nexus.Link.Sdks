using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

/// <summary>
/// The catch method for a try activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/> that has failed.</param>
/// <param name="exception">The exception for the failing activity.</param>
/// <param name="cancellationToken"></param>
public delegate Task TryCatchMethodAsync(IActivityAction activity, ActivityFailedException exception, CancellationToken cancellationToken);

/// <summary>
/// The implementation method for a try activity with no return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/> that has failed.</param>
/// <param name="exception">The exception for the failing activity.</param>
public delegate void TryCatchMethod(IActivityAction activity, ActivityFailedException exception);

/// <summary>
/// An activity where errors can be caught.
/// </summary>
public interface ICatchableActivity
{
    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/> that is of <paramref name="category"/>.
    /// </summary>
    /// <param name="category">The category of exceptions that will be caught.</param>
    /// <param name="methodAsync">The method to call when an exception is caught. The method is supposed to either throw or handle the error.</param>
    ITryCatchActivity Catch(ActivityExceptionCategoryEnum category, TryCatchMethodAsync methodAsync);

    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/> that is of <paramref name="category"/>.
    /// </summary>
    /// <param name="category">The category of exceptions that will be caught.</param>
    /// <param name="method">The method to call when an exception is caught. The method is supposed to either throw or handle the error.</param>
    ITryCatchActivity Catch(ActivityExceptionCategoryEnum category, TryCatchMethod method);
    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/>.
    /// </summary>
    /// <param name="methodAsync">The method to call when an exception is caught. The method is supposed to either throw or handle the error.</param>
    IExecutableActivity CatchAll(TryCatchMethodAsync methodAsync);

    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/>.
    /// </summary>
    /// <param name="method">The method to call when an exception is caught. The method is supposed to either throw or handle the error.</param>
    IExecutableActivity CatchAll(TryCatchMethod method);
}

/// <summary>
/// The catch method for a try activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/> that has failed.</param>
/// <param name="exception">The exception for the failing activity.</param>
/// <param name="cancellationToken"></param>
/// <typeparam name="TActivityReturns">The return type for the activity</typeparam>
public delegate Task<TActivityReturns> TryCatchMethodAsync<TActivityReturns>(IActivityAction<TActivityReturns> activity, ActivityFailedException exception, CancellationToken cancellationToken);

/// <summary>
/// The catch method for a try activity with a return value.
/// </summary>
/// <param name="activity">The current <see cref="IActivityAction"/> that has failed.</param>
/// <param name="exception">The exception for the failing activity.</param>
/// <typeparam name="TActivityReturns">The return type for the activity</typeparam>
public delegate TActivityReturns TryCatchMethod<TActivityReturns>(IActivityAction<TActivityReturns> activity, ActivityFailedException exception);

/// <summary>
/// An activity where errors can be caught.
/// </summary>
public interface ICatchableActivity<TActivityReturns>
{
    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/> that is of <paramref name="category"/>.
    /// </summary>
    /// <param name="category">The category of exceptions that will be caught.</param>
    /// <param name="methodAsync">The method to call when an exception is caught. The method is supposed to either throw or return the value to use.</param>
    ITryCatchActivity<TActivityReturns> Catch(ActivityExceptionCategoryEnum category, TryCatchMethodAsync<TActivityReturns> methodAsync);

    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/> that is of <paramref name="category"/>.
    /// </summary>
    /// <param name="category">The category of exceptions that will be caught.</param>
    /// <param name="method">The method to call when an exception is caught. The method is supposed to either throw or return the value to use.</param>
    ITryCatchActivity<TActivityReturns> Catch(ActivityExceptionCategoryEnum category, TryCatchMethod<TActivityReturns> method);
    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/>.
    /// </summary>
    /// <param name="methodAsync">The method to call when an exception is caught. The method is supposed to either throw or return the value to use.</param>
    IExecutableActivity<TActivityReturns> CatchAll(TryCatchMethodAsync<TActivityReturns> methodAsync);

    /// <summary>
    /// Catch any <see cref="ActivityFailedException"/>.
    /// </summary>
    /// <param name="method">The method to call when an exception is caught. The method is supposed to either throw or return the value to use.</param>
    IExecutableActivity<TActivityReturns> CatchAll(TryCatchMethod<TActivityReturns> method);
}