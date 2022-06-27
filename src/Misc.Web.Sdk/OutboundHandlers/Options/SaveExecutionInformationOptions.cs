using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Misc.Web.Sdk.OutboundHandlers.Options
{
    /// <summary>
    /// Forward header <see cref="Constants.FulcrumCorrelationIdHeaderName"/>
    /// </summary>
    public class SaveExecutionInformationOptions : Feature, IValidatable
    {
        /// <summary>
        /// A delegate to save information about a request execution before the actual execution
        /// </summary>
        public delegate Task SaveBeforeExecutionAsyncMethod(BeforeExecution beforeExecution, CancellationToken cancellationToken = default);

        /// <summary>
        /// A delegate to save information about a request execution after the actual execution
        /// </summary>
        public delegate Task SaveAfterExecutionAsyncMethod(AfterExecution afterExecution, CancellationToken cancellationToken = default);

        /// <summary>
        /// The method that will deal with the actual saving
        /// </summary>
        public SaveBeforeExecutionAsyncMethod SaveBeforeExecutionAsyncDelegate;

        /// <summary>
        /// The method that will deal with the actual saving
        /// </summary>
        public SaveAfterExecutionAsyncMethod SaveAfterExecutionAsyncDelegate;

        /// <summary>
        /// Information about a request execution before the actual execution
        /// </summary>
        public class BeforeExecution
        {
            /// <summary>
            /// The current execution id
            /// </summary>
            [Hint.PrimaryKey]
            [Validation.NotNullOrWhitespace]
            public string ExecutionId { get; set; }

            /// <summary>
            /// The execution id that created this <see cref="ExecutionId"/>, i.e. the parent of the execution id
            /// </summary>
            [CanBeNull]
            public string ParentExecutionId { get; set; }

            /// <summary>
            /// A summary of the request. For a REST request, this could be the HTTP Method and an anonymous version of the URL.
            /// </summary>
            [CanBeNull]
            public string RequestDescription { get; set; }
        }

        /// <summary>
        /// The different result types of a execution
        /// </summary>
        public enum ExecutionResultEnum
        {
            /// <summary>
            /// The request has been accepted. No information yet on success/fail
            /// </summary>
            Accepted,
            /// <summary>
            /// The request has completed and was successful.
            /// </summary>
            Success,
            /// <summary>
            /// The request execution has completed and failed. We can't say whether it is a temporary failure or not.
            /// </summary>
            Failure,
            /// <summary>
            /// The request execution has completed and failed. This is a temporary failure, i.e the execution can be retried.
            /// </summary>
            TemporaryFailure,
            /// <summary>
            /// The request execution has completed and failed. This is a permanent failure, i.e. no use in retrying,
            /// unless you change the request somehow.
            /// </summary>
            PermanentFailure
        }

        /// <summary>
        /// Information about a request execution before the actual execution
        /// </summary>
        public class AfterExecution
        {
            /// <summary>
            /// The current execution id
            /// </summary>
            [Hint.PrimaryKey]
            [Validation.NotNullOrWhitespace]
            public string ExecutionId { get; set; }

            /// <summary>
            /// The result for this execution
            /// </summary>
            public ExecutionResultEnum Result { get; set; }

            /// <summary>
            /// A summary of the response. For a REST request, this could be the HTTP status code and description, e.g. "200 OK".
            /// </summary>
            [CanBeNull]
            public string ResponseDescription { get; set; }

            /// <summary>
            /// The time span for executing the request.
            /// </summary>
            public TimeSpan Elapsed { get; set; }
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
        }
    }
}