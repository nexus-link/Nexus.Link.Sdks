using System;

namespace WorkflowEngine.Sdk.Inbound.RespondAsync
{
    // Allowed values for a respond async strategy
    public enum RespondAsyncOpinionEnum
    {
        // The method is fine with either way. This is the default.
        Indifferent = 0,
        // Never allow asynchronous response
        Never,
        // The method must always be executed asynchronously
        Always
    };
    /// <summary>
    /// Information about a translation concept
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RespondAsyncAttribute : Attribute
    {
        /// <summary>
        /// The respond async strategy
        /// </summary>
        public RespondAsyncOpinionEnum Opinion { get; }

        /// <summary>
        /// Set respond async options.
        /// </summary>
        public RespondAsyncAttribute(RespondAsyncOpinionEnum opinion)
        {
            Opinion = opinion;
        }
    }
}