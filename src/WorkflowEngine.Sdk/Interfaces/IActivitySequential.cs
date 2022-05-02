using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces;

/// <summary>
/// An activity of type <see cref="ActivityTypeEnum.Sequential"/>.
/// </summary>
public interface IActivitySequential : IActivityJobs<IActivitySequential>
{
}