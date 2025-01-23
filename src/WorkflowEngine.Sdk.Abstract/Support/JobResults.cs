using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using System;
using System.Collections.Concurrent;

namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.Support
{
    ///// <summary>
    ///// Interface to access individual job results.
    ///// </summary>
    //public interface IJobResults
    //{
    //    /// <summary>
    //    /// All the individual parallel jobs
    //    /// </summary>
    //    ConcurrentDictionary<int, JobResult> Results { get; set; }

    //    /// <summary>
    //    /// Get the result from a specific job.
    //    /// </summary>
    //    /// <param name="index">A reference to a specific job</param>
    //    /// <typeparam name="T">The type for the job result.</typeparam>
    //    /// <returns>The job result.</returns>
    //    T Get<T>(int index);
    //}

    /// <summary>
    /// Container for all <see cref="JobResult"/> from parallel execution.
    /// </summary>
    public class JobResults
    {
        /// <summary>
        /// All the individual parallel jobs
        /// </summary>
        public ConcurrentDictionary<int, JobResult> Results { get; set; } = new();
    }

    /// <summary>
    /// The result of one individual parallel job
    /// </summary>
    public class JobResult
    {
        /// <summary>
        /// The name of the C# type
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The result of the job, serialized to JSON
        /// </summary>
        public string ResultAsJson { get; set; }
    }
}