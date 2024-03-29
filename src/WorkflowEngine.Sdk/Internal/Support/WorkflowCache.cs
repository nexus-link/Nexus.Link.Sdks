﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Activity = Nexus.Link.WorkflowEngine.Sdk.Internal.Logic.Activity;
using Log = Nexus.Link.Libraries.Core.Logging.Log;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Support
{
    internal class WorkflowCache
    {
        private readonly IWorkflowInformation _workflowInformation;
        private readonly IWorkflowEngineRequiredCapabilities _workflowCapabilities;

        private readonly CrudPersistenceHelper<ActivityFormCreate, ActivityForm, string> _activityFormCache;

        private WorkflowSummary _summary;

        public WorkflowForm Form => _summary?.Form;

        public WorkflowVersion Version => _summary?.Version;

        public WorkflowInstance Instance => _summary?.Instance;

        private readonly NexusAsyncSemaphore _semaphore = new();
        private readonly CrudPersistenceHelper<ActivityVersionCreate, ActivityVersion, string> _activityVersionCache;
        private readonly CrudPersistenceHelper<ActivityInstanceCreate, ActivityInstance, string> _activityInstanceCache;
        private readonly CrudPersistenceHelper<WorkflowFormCreate, WorkflowForm, string> _workflowFormCache;
        private readonly CrudPersistenceHelper<WorkflowVersionCreate, WorkflowVersion, string> _workflowVersionCache;
        private readonly CrudPersistenceHelper<WorkflowInstanceCreate, WorkflowInstance, string> _workflowInstanceCache;

        private readonly Dictionary<string, Activity> _activities = new();
        private readonly IWorkflowStateCapability _stateCapability;

        public WorkflowCache(IWorkflowInformation workflowInformation,
            IWorkflowEngineRequiredCapabilities workflowCapabilities)
        {
            _workflowInformation = workflowInformation;
            _workflowCapabilities = workflowCapabilities;
            var crudPersistenceHelperOptions = new CrudPersistenceHelperOptions
            {
                ConflictStrategy = PersistenceConflictStrategyEnum.ReturnNew,
                OnlySequential = true
            };

            _stateCapability = workflowCapabilities.StateCapability;
            _workflowFormCache = new CrudPersistenceHelper<WorkflowFormCreate, WorkflowForm, string>(workflowCapabilities.ConfigurationCapability.WorkflowForm, crudPersistenceHelperOptions);
            _workflowVersionCache = new CrudPersistenceHelper<WorkflowVersionCreate, WorkflowVersion, string>(workflowCapabilities.ConfigurationCapability.WorkflowVersion, crudPersistenceHelperOptions);
            _activityFormCache = new CrudPersistenceHelper<ActivityFormCreate, ActivityForm, string>(workflowCapabilities.ConfigurationCapability.ActivityForm, crudPersistenceHelperOptions);
            _activityVersionCache = new CrudPersistenceHelper<ActivityVersionCreate, ActivityVersion, string>(
                workflowCapabilities.ConfigurationCapability.ActivityVersion,
                crudPersistenceHelperOptions,
                new ActivitySaveOrder());
            _workflowInstanceCache = new CrudPersistenceHelper<WorkflowInstanceCreate, WorkflowInstance, string>(workflowCapabilities.StateCapability.WorkflowInstance, crudPersistenceHelperOptions);
            _activityInstanceCache = new CrudPersistenceHelper<ActivityInstanceCreate, ActivityInstance, string>(
                workflowCapabilities.StateCapability.ActivityInstance,
                crudPersistenceHelperOptions,
                new ActivitySaveOrder());
        }

        public async Task<WorkflowSummary> LoadAsync(CancellationToken cancellationToken)
        {
            _summary = null;

            // Read from DB
            await ReadAndMaybeSaveSummaryAsync(cancellationToken);
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());
            _summary!.Form ??= new WorkflowForm
            {
                Id = _workflowInformation.FormId,
                CapabilityName = _workflowInformation.CapabilityName,
                Title = _workflowInformation.FormTitle
            };
            _summary.Version ??= new WorkflowVersion
            {
                Id = Guid.NewGuid().ToGuidString(),
                WorkflowFormId = _workflowInformation.FormId,
                MajorVersion = _workflowInformation.MajorVersion,
                MinorVersion = _workflowInformation.MinorVersion,
                DynamicCreate = true
            };
            _summary.Instance ??= new WorkflowInstance
            {
                Id = _workflowInformation.InstanceId,
                WorkflowVersionId = _summary.Version.Id,
                StartedAt = DateTimeOffset.UtcNow,
                InitialVersion = $"{_workflowInformation.MajorVersion}.{_workflowInformation.MinorVersion}",
                Title = _workflowInformation.InstanceTitle,
                State = WorkflowStateEnum.Waiting
            };
            return _summary;
        }

        private async Task ReadAndMaybeSaveSummaryAsync(CancellationToken cancellationToken)
        {
            await _semaphore.ExecuteAsync(async (ct) =>
            {
                // Read from DB
                _summary = await _stateCapability.WorkflowSummary.GetSummaryAsync(_workflowInformation.FormId, _workflowInformation.MajorVersion, _workflowInformation.InstanceId, ct);
                RememberData(true);
                if (_summary.Instance == null) return;

                // Check to see if an earlier execution failed to save to DB, but succeeded to save a fallback blob to storage
                try
                {
                    var summary =
                        await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.ReadBlobAsync(_summary.Instance.Id,
                            _summary.Instance.StartedAt, cancellationToken);
                    if (summary == null) return;
                    _summary = summary;
                }
                catch (FulcrumNotImplementedException)
                {
                    return;
                }

                // There was a fallback blob stored of the last state. Save that state to DB and remove the blob representation.
                await SaveAsync(false, cancellationToken);
                await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.DeleteBlobAsync(_summary.Instance.Id, _summary.Instance.StartedAt, cancellationToken);
            }, cancellationToken);
        }

        public async Task<WorkflowSummary> SaveAsync(bool useFallback, CancellationToken cancellationToken = default)
        {
            InternalContract.Require(_summary != null, $"The method {nameof(LoadAsync)} must be called before calling this method.");
            if (_summary == null) return _summary;
            RememberData(false);
            return await _semaphore.ExecuteAsync(async (ct) =>
            {
                var oldForm = _workflowFormCache.GetStored(_summary.Form.Id);
                var oldVersion = _workflowVersionCache.GetStored(_summary.Version.Id);
                var oldInstance = _workflowInstanceCache.GetStored(_summary.Instance.Id);

                try
                {
                    await SaveToDbAsync(ct);
                }
                catch (Exception ex)
                {
                    if (!useFallback) throw;
                    Log.LogWarning($"Failed to save to DB: {ex}\rWill try to save to secondary storage.");
                    await SaveToStorageAsync(ct);
                    throw new ActivityPostponedException(TimeSpan.FromSeconds(30));
                }

                if (_workflowInformation.WorkflowOptions.AfterSaveAsync != null)
                {
                    try
                    {
                        await _workflowInformation.WorkflowOptions.AfterSaveAsync(
                            oldForm, oldVersion, oldInstance, _summary.Form, _summary.Version, _summary.Instance);
                    }
                    catch (Exception e)
                    {
                        // Fail handling is deferred to implementor. E.g., if using AM, the AM SDK will provide retry mechanism.
                        Log.LogError(
                            $"Error at {nameof(_stateCapability.WorkflowInstance.DefaultWorkflowOptions.AfterSaveAsync)}: {e.Message}. Giving up.");
                    }
                }

                return _summary;
            }, cancellationToken);
        }

        private void RememberData(bool stored)
        {
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());
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

            foreach (var activityVersion in _summary.ActivityVersions.Values
                         .OrderBy(av => av, new ActivitySaveOrder()))
            {
                _activityVersionCache.Add(activityVersion.Id, activityVersion, stored);
            }

            foreach (var activityInstance in _summary.ActivityInstances.Values
                         .OrderBy(ai => ai, new ActivitySaveOrder()))
            {
                _activityInstanceCache.Add(activityInstance.Id, activityInstance, stored);
            }
        }

        /// <summary>
        /// We need to sort the activity versions so the versions that are referred as parents by
        /// other versions are saved first
        /// </summary>
        private class ActivitySaveOrder : IComparer<ActivityVersion>, IComparer<ActivityInstance>
        {
            /// <inheritdoc />
            public int Compare(ActivityVersion x, ActivityVersion y)
            {
                if (x == null || y == null) return 0;
                if (x.Id == y.ParentActivityVersionId) return -1;
                var preliminaryResult = 0;
                if (string.IsNullOrWhiteSpace(x.ParentActivityVersionId))
                {
                    preliminaryResult--;
                }
                else
                {
                    if (y.Id == x.ParentActivityVersionId) return 1;
                    preliminaryResult++;
                }
                if (string.IsNullOrWhiteSpace(y.ParentActivityVersionId))
                {
                    preliminaryResult++;
                }
                else
                {
                    if (x.Id == y.ParentActivityVersionId) return -1;
                    preliminaryResult--;
                }

                return preliminaryResult;
            }

            /// <inheritdoc />
            public int Compare(ActivityInstance x, ActivityInstance y)
            {
                if (x == null || y == null) return 0;
                if (x.Id == y.ParentActivityInstanceId) return -1;
                var preliminaryResult = 0;
                if (string.IsNullOrWhiteSpace(x.ParentActivityInstanceId))
                {
                    preliminaryResult--;
                }
                else
                {
                    if (y.Id == x.ParentActivityInstanceId) return 1;
                    preliminaryResult++;
                }
                if (string.IsNullOrWhiteSpace(y.ParentActivityInstanceId))
                {
                    preliminaryResult++;
                }
                else
                {
                    if (x.Id == y.ParentActivityInstanceId) return -1;
                    preliminaryResult--;
                }

                return preliminaryResult;
            }
        }

        private async Task SaveToDbAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());

            CancellationToken limitedToken;
            if (FulcrumApplication.IsInDevelopment)
            {
                limitedToken = cancellationToken;
            }
            else
            {
                // Max 10 s execution
                var limitedTimeCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var mergedToken =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                        limitedTimeCancellationToken.Token);
                limitedToken = mergedToken.Token;
            }


            using var scope = TransactionHelper.CreateStandardScope();
            await _workflowFormCache.SaveAsync((id, item) => _summary.Form = item, limitedToken);
            await _workflowVersionCache.SaveAsync((id, item) => _summary.Version = item, limitedToken);
            await _workflowInstanceCache.SaveAsync((id, item) => _summary.Instance = item, limitedToken);
            await _activityFormCache.SaveAsync((id, item) => _summary.ActivityForms[id] = item, limitedToken);
            await _activityVersionCache.SaveAsync((id, item) => _summary.ActivityVersions[id] = item, limitedToken);
            await _activityInstanceCache.SaveAsync((id, item) => _summary.ActivityInstances[id] = item, limitedToken);
            scope.Complete();
        }

        private async Task SaveToStorageAsync(CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());
            // Max 20 s retry
            var limitedTimeCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitedTimeCancellationToken.Token);
            var limitedToken = mergedToken.Token;
            Exception lastException = null;
            while (true)
            {
                try
                {
                    await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.WriteBlobAsync(_summary, limitedToken);
                    return;
                }
                catch (Exception ex)
                {
                    if (limitedToken.IsCancellationRequested || ex is FulcrumNotImplementedException || FulcrumApplication.IsInDevelopment)
                    {
                        var exception = lastException ?? ex;
                        throw exception;
                    }
                    lastException = ex;
                    await Task.Delay(1000, limitedToken);
                    // Try again
                }
            }
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
            InternalContract.RequireNotNull(activity, nameof(activity));
            var internalActivity = activity as Activity;
            FulcrumAssert.IsNotNull(internalActivity, CodeLocation.AsString());
            _activities[activityInstanceId] = internalActivity;
        }

        public bool TryGetActivity(string activityId, out Activity activity)
        {
            activity = null;
            return activityId != null && _activities.TryGetValue(activityId, out activity);
        }

        public bool TryGetActivity<TActivityReturns>(string activityId, out Activity<TActivityReturns> activity)
        {
            activity = null;
            if (!TryGetActivity(activityId, out var noResultActivity)) return false;
            activity = noResultActivity as Activity<TActivityReturns>;
            FulcrumAssert.IsNotNull(activity, CodeLocation.AsString());
            return true;
        }

        public string GetOrCreateInstanceId(IActivityInformation activityInformation)
        {
            lock (_summary)
            {
                var form = GetActivityForm(activityInformation.FormId);
                if (form == null)
                {
                    FulcrumAssert.IsNotNull(activityInformation.Type, CodeLocation.AsString());
                    form = new ActivityForm
                    {
                        Id = activityInformation.FormId,
                        WorkflowFormId = _workflowInformation.FormId,
                        Title = activityInformation.FormTitle,
                        Type = activityInformation.Type!.Value
                    };
                    _summary.ActivityForms.Add(form.Id, form);
                    _activityFormCache.Add(form.Id, form, false);

                }

                var version = GetActivityVersionByFormId(activityInformation.FormId);
                if (version == null)
                {
                    version = new ActivityVersion
                    {
                        Id = Guid.NewGuid().ToGuidString(),
                        WorkflowVersionId = _summary.Version.Id,
                        ActivityFormId = activityInformation.FormId,
                        FailUrgency = activityInformation.Options.FailUrgency,
                        ParentActivityVersionId = activityInformation.Parent?.Version?.Id,
                        Position = activityInformation.Position
                    };
                    _summary.ActivityVersions.Add(version.Id, version);
                    _activityVersionCache.Add(version.Id, version, false);
                }

                var instance = _summary.ActivityInstances.Values.FirstOrDefault(i => IsSameInstance(i, version));
                if (instance != null) return instance.Id;
                instance = new ActivityInstance
                {
                    Id = Guid.NewGuid().ToGuidString(),
                    WorkflowInstanceId = _workflowInformation.InstanceId,
                    ActivityVersionId = version.Id,
                    ParentActivityInstanceId = activityInformation.Parent?.ActivityInstanceId,
                    ParentIteration = activityInformation.Parent?.InternalIteration,
                    State = ActivityStateEnum.Executing,
                    StartedAt = DateTimeOffset.UtcNow
                };
                _summary.ActivityInstances.Add(instance.Id, instance);
                _activityInstanceCache.Add(instance.Id, instance, false);
                return instance.Id;
            }

            bool IsSameInstance(ActivityInstance instance, ActivityVersion version)
            {
                return instance.ActivityVersionId == version.Id
                       && instance.ParentActivityInstanceId == activityInformation.Parent?.Instance.Id
                       && instance.ParentIteration == activityInformation.Parent?.InternalIteration;
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