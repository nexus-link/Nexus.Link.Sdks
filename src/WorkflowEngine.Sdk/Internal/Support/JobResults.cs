using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support
{
    internal class JobResults : IJobResults
    {
        public ConcurrentDictionary<int, JobResult> Results = new();

        public void Add(int jobNumber, object result, Type type)
        {
            InternalContract.RequireGreaterThan(0, jobNumber, nameof(jobNumber));
            InternalContract.RequireNotNull(type, nameof(type));

            var success = Results.TryAdd(jobNumber, new JobResult
            {
                TypeName = type.Name,
                ResultAsJson = JsonConvert.SerializeObject(result)
            });
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
        }

        /// <inheritdoc />
        public T Get<T>(int index)
        {
            InternalContract.RequireGreaterThan(0, index, nameof(index));
            InternalContract.Require(Results.Keys.Contains(index), $"{nameof(index)} {index} was not found among the jobs that returns a result.");
            try
            {
                return JsonConvert.DeserializeObject<T>(Results[index].ResultAsJson);
            }
            catch (Exception e)
            {
                throw new FulcrumContractException(
                    $"The job result was of type {Results[index].TypeName}, which can't be converted to the type ({typeof(T).Name}).");
            }
        }
    }

    internal class JobResult
    {
        public string TypeName { get; set; }
        public string ResultAsJson { get; set; }
    }
}