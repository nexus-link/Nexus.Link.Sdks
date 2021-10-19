using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.MethodSupport;

namespace Nexus.Link.WorkflowEngine.Sdk.Model
{
    public class WorkflowInformation
    {
        public MethodHandler MethodHandler { get; }
        public IWorkflowCapability WorkflowCapability { get; }

        public WorkflowInformation(IWorkflowCapability workflowCapability, MethodHandler methodHandler)
        {
            MethodHandler = methodHandler;
            WorkflowCapability = workflowCapability;
        }

        // Form
        public string CapabilityName { get; set; }
        public string FormId { get; set; }
        public string FormTitle { get; set; }

        // Version
        public string VersionId { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }

        // Instance
        public string InstanceId { get; set; }
        public string InstanceTitle { get; set; }

        public string VersionTitle => $"{FormTitle} {MajorVersion}.{MinorVersion}";


        public async Task PersistAsync(CancellationToken cancellationToken)
        {
            await PersistFormAsync(cancellationToken);
            VersionId = await PersistVersionAsync(cancellationToken);
            await PersistInstanceAsync(cancellationToken);
            await MethodHandler.PersistWorkflowParametersAsync(WorkflowCapability, VersionId, cancellationToken);
        }

        private async Task PersistFormAsync(CancellationToken cancellationToken)
        {
            var workflowForm = await WorkflowCapability.WorkflowForm.ReadAsync(FormId, cancellationToken);
            if (workflowForm == null)
            {
                var workflowCreate = new WorkflowFormCreate
                {
                    CapabilityName = CapabilityName,
                    Title = FormTitle
                };
                await WorkflowCapability.WorkflowForm.CreateWithSpecifiedIdAsync(FormId, workflowCreate, cancellationToken);
            }
            else if (workflowForm.Title != FormTitle || workflowForm.CapabilityName != CapabilityName)
            {
                workflowForm.CapabilityName = CapabilityName;
                workflowForm.Title = FormTitle;
                await WorkflowCapability.WorkflowForm.UpdateAsync(FormId, workflowForm, cancellationToken);
            }
        }

        public async Task<string> PersistVersionAsync(CancellationToken cancellationToken)
        {
            var workflowVersion =
                await WorkflowCapability.WorkflowVersion.ReadAsync(FormId, MajorVersion,
                    cancellationToken);
            if (workflowVersion == null)
            {
                var workflowVersionCreate = new WorkflowVersionCreate
                {
                    WorkflowFormId = FormId,
                    MajorVersion = MajorVersion,
                    MinorVersion = MinorVersion,
                    DynamicCreate = true
                };
                await WorkflowCapability.WorkflowVersion.CreateWithSpecifiedIdAsync(
                    FormId, MajorVersion, workflowVersionCreate, cancellationToken); 
                
                workflowVersion = await WorkflowCapability.WorkflowVersion.ReadAsync(
                    FormId,
                    MajorVersion, cancellationToken);
            }
            else if (workflowVersion.MinorVersion != MinorVersion)
            {
                workflowVersion.MinorVersion = MinorVersion;
                await WorkflowCapability.WorkflowVersion.UpdateAsync(FormId,
                    MajorVersion, workflowVersion, cancellationToken);
            }

            return workflowVersion.Id;
        }

        private async Task PersistInstanceAsync(CancellationToken cancellationToken)
        {
            var workflowInstance = await WorkflowCapability.WorkflowInstance.ReadAsync(InstanceId, cancellationToken);
            if (workflowInstance == null)
            {
                var workflowCreate = new WorkflowInstanceCreate
                {
                    WorkflowVersionId = VersionId,
                    StartedAt = DateTimeOffset.UtcNow,
                    InitialVersion = $"{MajorVersion}.{MinorVersion}",
                    Title = InstanceTitle
                };
                try
                {
                    await WorkflowCapability.WorkflowInstance.CreateChildWithSpecifiedIdAsync(VersionId, InstanceId, workflowCreate, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // This is OK. Another thread has created the same Id after we did the read above.
                }
            }
        }
    }
}