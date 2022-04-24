using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    /// <summary>
    /// Common interface for both implementations that returns a value and for those that don't
    /// </summary>
    public interface IWorkflowImplementationBase : IWorkflowLogger
    {
        /// <summary>
        /// The major version for this implementation.
        /// </summary>
        int MajorVersion { get; }

        /// <summary>
        /// The minor version for this implementation
        /// </summary>
        int MinorVersion { get; }

        /// <summary>
        /// Return the title of the currently running instance
        /// </summary>
        string GetInstanceTitle();

        /// <summary>
        /// The workflow container for all implementations
        /// </summary>
        IWorkflowContainer WorkflowContainer { get; }

        /// <summary>
        /// The options for the 
        /// </summary>
        public ActivityOptions DefaultActivityOptions { get; }
    }



    /// <summary>
    /// An implementation that returns a result
    /// </summary>
    /// <typeparam name="TWorkflowResult">The type for the returned result.</typeparam>
    public interface IWorkflowImplementation<TWorkflowResult> : IWorkflowImplementationBase
    {
        /// <summary>
        /// Method for creating a new instance of the workflow implementation
        /// </summary>
        /// <returns>Returns the implementation. To be used for chaining.</returns>
        IWorkflowImplementation<TWorkflowResult> CreateWorkflowInstance();

        /// <summary>
        /// Set an input parameter for the workflow instance before it executes.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The parameter value</param>
        /// <returns>Returns the implementation. To be used for chaining.</returns>
        IWorkflowImplementation<TWorkflowResult> SetParameter<TParameter>(string name, TParameter value);

        /// <summary>
        /// Execute the current instance of the workflow implementation
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task<TWorkflowResult> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// An implementation that doesn't return a result
    /// </summary>
    public interface IWorkflowImplementation : IWorkflowImplementationBase
    {
        /// <summary>
        /// Method for creating a new instance of the workflow implementation
        /// </summary>
        /// <returns>Returns the implementation. To be used for chaining.</returns>
        IWorkflowImplementation CreateWorkflowInstance();

        /// <summary>
        /// Set an input parameter for the workflow instance before it executes.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter</typeparam>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The parameter value</param>
        /// <returns>Returns the implementation. To be used for chaining.</returns>
        IWorkflowImplementation SetParameter<TParameter>(string name, TParameter value);

        /// <summary>
        /// Execute the current instance of the workflow implementation
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}