using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Extensions;

/// <summary>
/// Extensions for <see cref="JobResults"/>
/// </summary>
public static class JobResultsExtensions
{
    /// <summary>
    /// Get the result from a specific job.
    /// </summary>
    /// <param name="jobResults">The <see cref="JobResults"/> that we want to get data from.</param>
    /// <param name="index">A reference to a specific job</param>
    /// <typeparam name="T">The type for the job result.</typeparam>
    /// <returns>The job result.</returns>
    public static T Get<T>(this JobResults jobResults, int index)
    {
        InternalContract.RequireGreaterThan(0, index, nameof(index));
        InternalContract.Require(jobResults.Results.Keys.Contains(index), $"{nameof(index)} {index} was not found among the jobs that returns a result.");
        try
        {
            return JsonConvert.DeserializeObject<T>(jobResults.Results[index].ResultAsJson);
        }
        catch (Exception e)
        {
            throw new FulcrumContractException(
                $"The job result was of type {jobResults.Results[index].TypeName}, which can't be converted to the type ({typeof(T).Name}): {e.Message}");
        }
    }
    internal static void Add(this JobResults jobResults, int jobNumber, object result, Type type)
    {
        InternalContract.RequireGreaterThan(0, jobNumber, nameof(jobNumber));
        InternalContract.RequireNotNull(type, nameof(type));

        var success = jobResults.Results.TryAdd(jobNumber, new JobResult
        {
            TypeName = type.Name,
            ResultAsJson = JsonConvert.SerializeObject(result)
        });
        FulcrumAssert.IsTrue(success, CodeLocation.AsString());
    }
}