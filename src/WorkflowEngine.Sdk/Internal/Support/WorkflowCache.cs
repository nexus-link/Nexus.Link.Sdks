using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Model;
using WorkflowVersion = Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities.WorkflowVersion;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support
{
    internal class WorkflowCache
    {
        private readonly WorkflowInformation _workflowInformation;

        private readonly CrudPersistenceHelper<ActivityFormCreate, ActivityForm, string> _activityFormCache;

        private WorkflowSummary _summary;

        public WorkflowForm Form
        {
            get => _summary?.Form;
            set => _summary.Form = value;
        }

        public WorkflowVersion Version
        {
            get => _summary?.Version;
            set => _summary.Version = value;
        }
        public WorkflowInstance Instance
        {
            get => _summary?.Instance;
            set => _summary.Instance = value;
        }

        public Activity LatestActivity { get; set; }

        private readonly NexusAsyncSemaphore _semaphore = new();
        private readonly CrudPersistenceHelper<ActivityVersionCreate, ActivityVersion, string> _activityVersionCache;
        private readonly CrudPersistenceHelper<ActivityInstanceCreate, ActivityInstance, string> _activityInstanceCache;
        private readonly CrudPersistenceHelper<WorkflowFormCreate, WorkflowForm, string> _workflowFormCache;
        private readonly CrudPersistenceHelper<WorkflowVersionCreate, WorkflowVersion, string> _workflowVersionCache;
        private readonly CrudPersistenceHelper<WorkflowInstanceCreate, WorkflowInstance, string> _workflowInstanceCache;

        private readonly Dictionary<string, Activity> _activities = new();

        public WorkflowCache(WorkflowInformation workflowInformation)
        {
            _workflowInformation = workflowInformation;
            var crudPersistenceHelperOptions = new CrudPersistenceHelperOptions
            {
                ConflictStrategy = PersistenceConflictStrategyEnum.ReturnNew
            };

            _workflowFormCache = new CrudPersistenceHelper<WorkflowFormCreate, WorkflowForm, string>(_workflowInformation.WorkflowCapabilities.ConfigurationCapability.WorkflowForm, crudPersistenceHelperOptions);
            _workflowVersionCache = new CrudPersistenceHelper<WorkflowVersionCreate, WorkflowVersion, string>(_workflowInformation.WorkflowCapabilities.ConfigurationCapability.WorkflowVersion, crudPersistenceHelperOptions);
            _activityFormCache = new CrudPersistenceHelper<ActivityFormCreate, ActivityForm, string>(_workflowInformation.WorkflowCapabilities.ConfigurationCapability.ActivityForm, crudPersistenceHelperOptions);
            _activityVersionCache = new CrudPersistenceHelper<ActivityVersionCreate, ActivityVersion, string>(_workflowInformation.WorkflowCapabilities.ConfigurationCapability.ActivityVersion, crudPersistenceHelperOptions);
            _workflowInstanceCache = new CrudPersistenceHelper<WorkflowInstanceCreate, WorkflowInstance, string>(_workflowInformation.WorkflowCapabilities.StateCapability.WorkflowInstance, crudPersistenceHelperOptions);
            _activityInstanceCache = new CrudPersistenceHelper<ActivityInstanceCreate, ActivityInstance, string>(_workflowInformation.WorkflowCapabilities.StateCapability.ActivityInstance, crudPersistenceHelperOptions);
        }

        public async Task<WorkflowSummary> LoadAsync(CancellationToken cancellationToken)
        {
            if (_summary != null) return _summary;
            return await _semaphore.ExecuteAsync(async (ct) =>
            {
                if (_summary != null) return _summary;
                _summary = await _workflowInformation.WorkflowCapabilities.StateCapability.WorkflowSummary.GetSummaryAsync(
                    _workflowInformation.FormId, _workflowInformation.MajorVersion, _workflowInformation.InstanceId, ct);
                RememberData(true);
                if (_summary.Form == null)
                {
                    _summary.Form = new WorkflowForm
                    {
                        Id = _workflowInformation.FormId,
                        CapabilityName = _workflowInformation.CapabilityName,
                        Title = _workflowInformation.FormTitle
                    };
                }
                if (_summary.Version == null)
                {
                    _summary.Version = new WorkflowVersion
                    {
                        Id = Guid.NewGuid().ToGuidString(),
                        WorkflowFormId = _workflowInformation.FormId,
                        MajorVersion = _workflowInformation.MajorVersion,
                        MinorVersion = _workflowInformation.MinorVersion,
                        DynamicCreate = true
                    };
                }
                if (_summary.Instance == null)
                {
                    _summary.Instance = new WorkflowInstance
                    {
                        Id = _workflowInformation.InstanceId,
                        WorkflowVersionId = _summary.Version.Id,
                        StartedAt = DateTimeOffset.UtcNow,
                        InitialVersion = $"{_workflowInformation.MajorVersion}.{_workflowInformation.MinorVersion}",
                        Title = _workflowInformation.InstanceTitle,
                        State = WorkflowStateEnum.Executing,
                    };
                }
                return _summary;
            }, cancellationToken);

        }

        public async Task<WorkflowSummary> SaveAsync(CancellationToken cancellationToken = default)
        {
            InternalContract.Require(_summary != null, $"The method {nameof(LoadAsync)} must be called before calling this method.");
            if (_summary == null) return _summary;
            RememberData(false);
            return await _semaphore.ExecuteAsync(async (ct) =>
            {
                await InternalSaveAsync(ct);
                return _summary;
            }, cancellationToken);
        }

        private void RememberData(bool stored)
        {
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());
            var tasks = new List<Task>();
            if (_summary.Form != null)
            {
                _workflowFormCache.Add(_summary.Form.Id, _summary.Form, stored);
            }
            if (_summary.Version != null)
            {
                _workflowVersionCache.Add(_summary.Version.Id, _summary.Version, stored);
            }
            if (_summary.Instance != null)
            {
                _workflowInstanceCache.Add(_summary.Instance.Id, _summary.Instance, stored);
            }

            foreach (var activityForm in _summary.ActivityForms.Values)
            {
                _activityFormCache.Add(activityForm.Id, activityForm, stored);
            }

            foreach (var activityVersion in _summary.ActivityVersions.Values)
            {
                _activityVersionCache.Add(activityVersion.Id, activityVersion, stored);
            }

            foreach (var activityInstance in _summary.ActivityInstances.Values)
            {
                _activityInstanceCache.Add(activityInstance.Id, activityInstance, stored);
            }
        }

        private async Task InternalSaveAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());

            await _workflowFormCache.SaveAsync((id, item) => _summary.Form = item, cancellationToken);

            await _workflowVersionCache.SaveAsync((id, item) => _summary.Version = item, cancellationToken);

            await _workflowInstanceCache.SaveAsync((id, item) => _summary.Instance = item, cancellationToken);

            await _activityFormCache.SaveAsync((id, item) => _summary.ActivityForms[id] = item, cancellationToken);

            await _activityVersionCache.SaveAsync((id, item) => _summary.ActivityVersions[id] = item, cancellationToken);

            await _activityInstanceCache.SaveAsync((id, item) => _summary.ActivityInstances[id] = item, cancellationToken);
        }

        public ActivityForm GetActivityForm(string formId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            if (!_summary.ActivityForms.ContainsKey(formId)) return null;
            return _summary.ActivityForms[formId];
        }

        public ActivityVersion GetActivityVersion(string versionId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(versionId, nameof(versionId));
            if (!_summary.ActivityVersions.ContainsKey(versionId)) return null;
            return _summary.ActivityVersions[versionId];
        }

        public ActivityVersion GetActivityVersionByFormId(string formId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            return _summary.ActivityVersions.Values.FirstOrDefault(av => av.ActivityFormId == formId);
        }

        public ActivityInstance GetActivityInstance(string instanceId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));
            if (!_summary.ActivityInstances.ContainsKey(instanceId)) return null;
            return _summary.ActivityInstances[instanceId];
        }

        public void AddActivity(string activityInstanceId, IActivity activity)
        {
            var internalActivity = activity as Activity;
            FulcrumAssert.IsNotNull(internalActivity, CodeLocation.AsString());
            _activities[activityInstanceId] = internalActivity;
        }

        public Activity GetActivity(string activityId)
        {
            if (activityId == null) return null;
            var success = _activities.TryGetValue(activityId, out var activity);
            FulcrumAssert.IsTrue(success, CodeLocation.AsString());
            return activity;
        }

        public Activity GetCurrentParentActivity()
        {
            if (WorkflowStatic.Context.ParentActivityInstanceId == null) return null;
            var parentActivity = GetActivity(WorkflowStatic.Context.ParentActivityInstanceId);
            FulcrumAssert.IsNotNull(parentActivity, CodeLocation.AsString());
            return parentActivity;
        }

        public string GetOrCreateInstanceId(ActivityTypeEnum activityTypeEnum, IInternalActivityFlow activityFlow,
            IActivity parentActivity)
        {
            var internalParentActivity = parentActivity as Activity;
            if (parentActivity != null)
            {
                FulcrumAssert.IsNotNull(internalParentActivity, CodeLocation.AsString());
            }
            lock (_summary)
            {
                var form = GetActivityForm(activityFlow.ActivityFormId);
                if (form == null)
                {
                    form = new ActivityForm
                    {
                        Id = activityFlow.ActivityFormId,
                        WorkflowFormId = _workflowInformation.FormId,
                        Title = activityFlow.FormTitle,
                        Type = activityTypeEnum
                    };
                    _summary.ActivityForms.Add(form.Id, form);
                    _activityFormCache.Add(form.Id, form, false);

                }

                var version = GetActivityVersionByFormId(activityFlow.ActivityFormId);
                if (version == null)
                {
                    version = new ActivityVersion
                    {
                        Id = Guid.NewGuid().ToGuidString(),
                        WorkflowVersionId = _summary.Version.Id,
                        ActivityFormId = activityFlow.ActivityFormId,
                        FailUrgency = activityFlow.Options.FailUrgency,
                        ParentActivityVersionId = internalParentActivity?.Version?.Id,
                        Position = activityFlow.Position
                    };
                    _summary.ActivityVersions.Add(version.Id, version);
                    _activityVersionCache.Add(version.Id, version, false);
                }

                var instance = _summary.ActivityInstances.Values.FirstOrDefault(i =>
                    i.ActivityVersionId == version.Id
                    && i.ParentActivityInstanceId == internalParentActivity?.Instance.Id
                    && i.ParentIteration == parentActivity?.Iteration);
                if (instance != null) return instance.Id;
                instance = new ActivityInstance
                {
                    Id = Guid.NewGuid().ToGuidString(),
                    WorkflowInstanceId = _workflowInformation.InstanceId,
                    ActivityVersionId = version.Id,
                    ParentActivityInstanceId = parentActivity?.ActivityInstanceId,
                    ParentIteration = parentActivity?.Iteration,
                    State = ActivityStateEnum.Executing,
                    StartedAt = DateTimeOffset.UtcNow
                };
                _summary.ActivityInstances.Add(instance.Id, instance);
                _activityInstanceCache.Add(instance.Id, instance, false);
                return instance.Id;
            }
        }

        public bool InstanceExists() => Instance?.Etag != null;

        public void AggregateActivityInformation()
        {
            var executing = 0;
            var waiting = 0;
            var success = 0;
            var failedAndStopping = 0;
            var failedAndLater = 0;
            var failedAndIgnore = 0;
            var cancelWorkflow = false;
            foreach (var activity in _activities.Values)
            {
                switch (activity.Instance.State)
                {
                    case ActivityStateEnum.Executing:
                        executing++;
                        break;
                    case ActivityStateEnum.Waiting:
                        waiting++;
                        break;
                    case ActivityStateEnum.Success:
                        success++;
                        break;
                    case ActivityStateEnum.Failed:
                        switch (activity.Version.FailUrgency)
                        {
                            case ActivityFailUrgencyEnum.Stopping:
                                failedAndStopping++;
                                break;
                            case ActivityFailUrgencyEnum.CancelWorkflow:
                                cancelWorkflow = true;
                                break;
                            case ActivityFailUrgencyEnum.HandleLater:
                                failedAndLater++;
                                break;
                            case ActivityFailUrgencyEnum.Ignore:
                                failedAndIgnore++;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (Instance.State == WorkflowStateEnum.Success)
            {
                Instance.IsComplete = failedAndLater == 0;
                FulcrumAssert.IsNotNull(Instance.FinishedAt, CodeLocation.AsString());
            }
            else if (failedAndStopping > 0)
            {
                Instance.State = waiting > 0 ? WorkflowStateEnum.Halting : WorkflowStateEnum.Halted;
            }
            else if (waiting > 0)
            {
                Instance.State = WorkflowStateEnum.Waiting;
            }
            else
            {
                Instance.State = cancelWorkflow ? WorkflowStateEnum.Failed : WorkflowStateEnum.Success;
                Instance.FinishedAt = DateTimeOffset.UtcNow;
                if (cancelWorkflow && Instance.CancelledAt == null) Instance.CancelledAt = Instance.FinishedAt;
            }
        }
    }
}