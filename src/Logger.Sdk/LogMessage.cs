﻿using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;

// ReSharper disable once CheckNamespace
namespace Nexus.Link.Logger.Sdk
{
    /// <summary>
    /// Represents a log message with properties such as correlation id, calling client, severity and the text message.
    /// </summary>
    public class LogMessage : IValidatable
    {
        /// <summary>
        /// Tenant.Organization
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Tenant.Environment
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// The name of the service that the logging is done from
        /// </summary>
        public string Originator { get; set; }

        /// <summary>
        /// The name of the calling system
        /// </summary>
        public string CallingClientName { get; set; }

        /// <summary>
        /// A correlation id that ties this log message together in different systems
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Timestamp in UTC when the log message was created
        /// </summary>
        [Obsolete("Use Timestamp", false)] // TODO: Remove this field when all services are able to provide Timestamp
        public DateTimeOffset UtcDateTimeOffset
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        /// <summary>
        /// Timestamp when the log message was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The <see cref="LogSeverityLevel"/> of the log message
        /// </summary>
        public LogSeverityLevel SeverityLevel { get; set; }

        /// <summary>
        /// The logged message in plain text
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Where the log was issued (typically file name and line number)
        /// </summary>
        public string Location { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            // Note! Don't check Org/Env here, since they can be null for yet-to-be-discovered reasons
            FulcrumValidate.IsNotNullOrWhiteSpace(Originator, nameof(Originator), errorLocation);
            FulcrumValidate.IsNotNull(Timestamp, nameof(Timestamp), errorLocation);
            FulcrumValidate.IsNotDefaultValue(Timestamp, nameof(Timestamp), errorLocation);
            FulcrumValidate.IsNotNull(SeverityLevel, nameof(SeverityLevel), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Organization, nameof(Organization), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Environment, nameof(Environment), errorLocation);
        }
    }
}
