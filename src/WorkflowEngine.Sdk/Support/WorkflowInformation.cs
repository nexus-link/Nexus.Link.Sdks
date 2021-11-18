using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.AsyncManager.Sdk;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public class WorkflowInformation : IValidatable, IWorkflowLogger
    {
        public IWorkflowImplementationBase WorkflowImplementation { get; set; }

        public IWorkflowVersions WorkflowVersions =>
            WorkflowImplementation.WorkflowVersions;

        public IAsyncRequestMgmtCapability AsyncRequestMgmtCapability => WorkflowVersions.AsyncRequestMgmtCapability;

        public IWorkflowMgmtCapability WorkflowCapability => WorkflowVersions.WorkflowCapability;
        public string CapabilityName => WorkflowVersions.WorkflowCapabilityName;
        public string FormTitle => WorkflowVersions.WorkflowFormTitle;
        public string InstanceTitle => WorkflowImplementation.GetInstanceTitle();
        public int MajorVersion => WorkflowImplementation.MajorVersion;
        public int MinorVersion => WorkflowImplementation.MinorVersion;
        public string FormId { get; }
        public string InstanceId { get; set; }
        public ActivityFailUrgencyEnum DefaultFailUrgency { get; set; } = ActivityFailUrgencyEnum.Stopping;
        public ActivityExceptionAlertHandler DefaultExceptionAlertHandler { get; set; }
        public double DefaultAsyncRequestPriority { get; set; } = 0.5;

        public WorkflowInformation(IWorkflowImplementationBase workflowImplementation)
        {
            WorkflowImplementation = workflowImplementation;
            FormId = WorkflowVersions.WorkflowFormId.ToLowerInvariant();
        }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(WorkflowCapability, nameof(WorkflowCapability), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(CapabilityName, nameof(CapabilityName), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(FormId, nameof(FormId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(FormTitle, nameof(FormTitle), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MajorVersion, nameof(MajorVersion), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0, MinorVersion, nameof(MinorVersion), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceId, nameof(InstanceId), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceTitle, nameof(InstanceTitle), errorLocation);
        }

        /// <inheritdoc />
        public async Task LogAtLevelAsync(LogSeverityLevel severityLevel, string message, object data, CancellationToken cancellationToken)
        {
            try
            {
                FulcrumAssert.IsNotNull(WorkflowCapability, CodeLocation.AsString());
                var jToken = WorkflowStatic.SafeConvertToJToken(data);
                var log = new LogCreate
                {
                    WorkflowFormId = FormId,
                    WorkflowInstanceId = InstanceId,
                    ActivityFormId = null,
                    SeverityLevel = severityLevel,
                    Message = message,
                    Data = jToken,
                    TimeStamp = DateTimeOffset.UtcNow,
                };
                await WorkflowCapability.Log.CreateAsync(log, cancellationToken);
            }
            catch (Exception)
            {
                if (FulcrumApplication.IsInDevelopment) throw;
                // Ignore logging problems when not in development mode.
            }
        }
    }
}