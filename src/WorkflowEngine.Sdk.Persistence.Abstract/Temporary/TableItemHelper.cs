using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;

namespace WorkflowEngine.Persistence.Abstract.Temporary
{
    /// <summary>
    /// Helper class for classes that implements <see cref="ICompleteTableItem"/>.
    /// </summary>
    public static class TableItemHelper
    {
        /// <summary>
        /// All classes that implements <see cref="ICompleteTableItem"/> should call this method to validate the mandatory fields.
        /// </summary>
        public static void Validate(ICompleteTableItem myTableItem, bool validateTimeStamps, string errorLocation, string propertyPath)
        {
            // Id != Guid.Empty
            FulcrumValidate.AreNotEqual(Guid.Empty, myTableItem.Id, nameof(myTableItem.Id), errorLocation);

            // Etag has a value
            FulcrumValidate.IsNotNullOrWhiteSpace(myTableItem.Etag, nameof(myTableItem.Etag), errorLocation);

            if (validateTimeStamps)
            {
                // 1970 < RecordCreatedAt <= RecordUpdatedAt <= now()
                FulcrumValidate.IsGreaterThan(DateTimeOffset.FromUnixTimeSeconds(0), myTableItem.RecordCreatedAt,
                    nameof(myTableItem.RecordCreatedAt), errorLocation);
                if (FulcrumApplication.IsInDevelopment)
                {
                    FulcrumValidate.IsGreaterThanOrEqualTo(
                        myTableItem.RecordCreatedAt.Subtract(TimeSpan.FromSeconds(1)), myTableItem.RecordUpdatedAt,
                        nameof(myTableItem.RecordUpdatedAt), errorLocation);
                    FulcrumValidate.IsLessThanOrEqualTo(DateTimeOffset.UtcNow.AddSeconds(1),
                        myTableItem.RecordUpdatedAt, nameof(myTableItem.RecordUpdatedAt), errorLocation);
                }
            }
        }
    }
}