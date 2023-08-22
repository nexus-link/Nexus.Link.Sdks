using System;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    /// <summary>
    /// Use this exception if your activity failed.
    /// </summary>
    [Obsolete($"Please use {nameof(ActivityFailedException)}. Obsolete since 2022-04-26.")]
    public class ActivityException : Exception
    {
        /// <summary>
        /// The failure category 
        /// </summary>
        public ActivityExceptionCategoryEnum ExceptionCategory { get; }

        /// <summary>
        /// A message directed to a developer
        /// </summary>
        public string TechnicalMessage { get; }

        /// <summary>
        /// A message directed to a business person
        /// </summary>
        public string FriendlyMessage { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exceptionCategory">The failure category</param>
        /// <param name="technicalMessage">A message directed to a developer</param>
        /// <param name="friendlyMessage">A message directed to a business person</param>
        public ActivityException(ActivityExceptionCategoryEnum exceptionCategory, string technicalMessage, string friendlyMessage)
        :base(technicalMessage)
        {
            ExceptionCategory = exceptionCategory;
            TechnicalMessage = technicalMessage;
            FriendlyMessage = friendlyMessage;
        }
    }
}