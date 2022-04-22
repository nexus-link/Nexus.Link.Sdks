namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// Interface to access individual job results.
    /// </summary>
    public interface IJobResults
    {
        /// <summary>
        /// Get the result from a specific job.
        /// </summary>
        /// <param name="index">A reference to a specific job</param>
        /// <typeparam name="T">The type for the job result.</typeparam>
        /// <returns>The job result.</returns>
        T Get<T>(int index);
    }
}