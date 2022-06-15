namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

/// <summary>
/// An activity that is executable and where errors can be caught.
/// </summary>
public interface ITryCatchActivity : IExecutableActivity, ICatchableActivity
{
}

/// <summary>
/// An activity that is executable and where errors can be caught.
/// </summary>
public interface ITryCatchActivity<TActivityReturns> : IExecutableActivity<TActivityReturns>, ICatchableActivity<TActivityReturns>
{
}