using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

/// <summary>
/// Global workflow options
/// </summary>
public class WorkflowOptions
{
    /// <summary>
    /// After a <see cref="WorkflowInstance"/> has been saved,
    /// this will be called with the old versions of the form, version and instance
    /// along with the new versions, enabling us to compare them and take action.
    /// </summary>
    public AfterSaveDelegate AfterSaveAsync { get; set; }

    /// <summary>
    /// Support for important json features such as Serialize and Deserialize.
    /// </summary>
    public IJsonSupport JsonSupport { get; set; }

    /// <summary>
    /// Copy the options from <paramref name="source"/>.
    /// </summary>
    public WorkflowOptions From(WorkflowOptions source)
    {
        AfterSaveAsync = source.AfterSaveAsync;
        JsonSupport = source.JsonSupport;
        return this;
    }

}