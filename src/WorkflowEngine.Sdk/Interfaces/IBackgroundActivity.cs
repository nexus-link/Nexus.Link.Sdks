using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity that supports waiting for background execution results.
/// </summary>
public interface IBackgroundActivity : IActivity
{
}

/// <summary>
/// An activity that supports waiting for background execution results.
/// </summary>
public interface IBackgroundActivity<TActivityResult> : IActivity
{
}