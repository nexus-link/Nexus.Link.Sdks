using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Service for dealing with with activity instances
/// </summary>
public interface IActivityInstanceService : 
    ICreateWithSpecifiedIdAndReturn<ActivityInstanceCreate,ActivityInstance, string>,
    IRead<ActivityInstance, string>,
    IUpdateAndReturn<ActivityInstance, string>
{
    /// <summary>
    /// Set the value of a context variable
    /// </summary>
    /// <param name="id">The activity instance</param>
    /// <param name="key">The name of the variable</param>
    /// <param name="content">The value for the variable</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetContextAsync(string id, string key, JToken content, CancellationToken cancellationToken = default);
}