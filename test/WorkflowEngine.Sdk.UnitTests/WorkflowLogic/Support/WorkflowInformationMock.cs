using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using Nexus.Link.WorkflowEngine.Sdk.Support;

namespace WorkflowEngine.Sdk.UnitTests.WorkflowLogic.Support
{
    internal class WorkflowInformationMock : IWorkflowInformation
    {
        private ActivityInstance _activityInstance;
        private ActivityVersion _activityVersion;
        private ActivityForm _activityForm;

        /// <inheritdoc />
        public string CapabilityName { get; set; } = "Capability name";

        /// <inheritdoc />
        public string FormId { get; set; } = "1BF92C6F-CB6E-44B3-B9AF-1FA9E15DC732";

        /// <inheritdoc />
        public string FormTitle { get; set; } = "Form title";

        /// <inheritdoc />
        public WorkflowForm Form { get; set; } = new WorkflowForm();

        /// <inheritdoc />
        public int MajorVersion { get; set; } = 1;

        /// <inheritdoc />
        public int MinorVersion { get; set; } = 0;

        /// <inheritdoc />
        public WorkflowVersion Version { get; set; } = new WorkflowVersion();

        /// <inheritdoc />
        public string InstanceId { get; set; } = "44286249-FDDE-40AD-860C-89F49FF92792";

        /// <inheritdoc />
        public WorkflowInstance Instance { get; set; } = new WorkflowInstance();

        /// <inheritdoc />
        public string InstanceTitle { get; set; } = "Instance title";

        /// <inheritdoc />
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;

        /// <inheritdoc />
        public ILogService LogService { get; set; } = null;

        /// <inheritdoc />
        public IWorkflowSemaphoreService SemaphoreService { get; set; } = null;

        /// <inheritdoc />
        public IWorkflowInstanceService WorkflowInstanceService { get; set; } = null;

        /// <inheritdoc />
        public ActivityOptions DefaultActivityOptions { get; set; }

        /// <inheritdoc />
        public Activity LatestActivity { get; set; }

        /// <inheritdoc />
        public ICollection<string> ActivitiesToPurge { get; set; } = new List<string>();

        /// <inheritdoc />
        public ActivityDefinition GetActivityDefinition(string activityFormId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ActivityForm GetActivityForm(string activityFormId)
        {
            _activityForm ??= new ActivityForm
            {
                Id = activityFormId,
                WorkflowFormId = "7D6D75AB-C21E-45A0-9146-10686705052D",
                Etag = "made up",
                Title = "Activity form title",
                Type = ActivityTypeEnum.Action,
            };
            return _activityForm;
        }

        /// <inheritdoc />
        public ActivityVersion GetActivityVersionByFormId(string activityFormId)
        {
            _activityVersion ??= new ActivityVersion
            {
                Id = "9873796C-CC20-434B-9A54-00081BC24712",
                ActivityFormId = activityFormId,
                Etag = "made up",
                FailUrgency = ActivityFailUrgencyEnum.Stopping,
                Position = 1,
                WorkflowVersionId = "3887E959-DA98-4B53-9534-C0F49A27FC08",
                ParentActivityVersionId = null
            };
            return _activityVersion;
        }

        /// <inheritdoc />
        public ActivityInstance GetActivityInstance(string activityInstanceId)
        {
            _activityInstance ??= new ActivityInstance
            {
                Id = activityInstanceId,
                WorkflowInstanceId = InstanceId,
                StartedAt = StartedAt,
                ActivityVersionId = "D21EB356-C401-4A04-9469-12668DE28AE5",
                AsyncRequestId = null,
                ContextDictionary = new ConcurrentDictionary<string, JToken>(),
                Etag = "made up",
                State = ActivityStateEnum.Executing
            };

            return _activityInstance;
        }

        /// <inheritdoc />
        public Activity GetCurrentParentActivity()
        {
            return null;
        }

        /// <inheritdoc />
        public void AddActivity(Activity activity)
        {
        }

        /// <inheritdoc />
        public Activity GetActivity(string activityInstanceId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetOrCreateInstanceId(IActivityInformation activityInformation)
        {
            return "1EA54949-94C6-469E-857F-E16EC216D498";
        }

        /// <inheritdoc />
        public Task LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task SaveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool InstanceExists()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AggregateActivityInformation()
        {
            throw new NotImplementedException();
        }
    }
}