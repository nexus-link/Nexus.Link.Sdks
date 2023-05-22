using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.WorkflowEngine.Sdk.Exceptions
{
    /// <summary>
    /// Use this exception if your activity failed.
    /// </summary>
#pragma warning disable CS0618
    public class ActivityFailedException : ActivityException
#pragma warning restore CS0618
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exceptionCategory">The failure category</param>
        /// <param name="technicalMessage">A message directed to a developer</param>
        /// <param name="friendlyMessage">A message directed to a business person</param>
        public ActivityFailedException(ActivityExceptionCategoryEnum exceptionCategory, string technicalMessage, string friendlyMessage) : base(exceptionCategory, technicalMessage, friendlyMessage)
        {
        }

        /// <inheritdoc />
        public override string ToString() => $"{ExceptionCategory} {TechnicalMessage}";

        /// <summary>
        /// Serialize this exception
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var o = new Serialized
            {
                ExceptionCategory = ExceptionCategory.ToString(),
                TechnicalMessage = TechnicalMessage, 
                FriendlyMessage = FriendlyMessage
            };
            return o.ToJsonString();
        }

        /// <summary>
        /// Deserialize <paramref name="serialized"></paramref> into this exception
        /// </summary>
        public static ActivityFailedException Deserialize(string serialized)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serialized, nameof(serialized));
            var o = serialized.ToObjectFromJson<Serialized>();
            var e = new ActivityFailedException(
                o.ExceptionCategory.ToEnum<ActivityExceptionCategoryEnum>(),
                o.TechnicalMessage,
                o.FriendlyMessage);
            return e;
        }

        private class Serialized
        {
            public string ExceptionCategory { get; set; }
            public string TechnicalMessage { get; set; }
            public string FriendlyMessage { get; set; }
        }
    }
}