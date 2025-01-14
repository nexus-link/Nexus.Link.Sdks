namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;

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