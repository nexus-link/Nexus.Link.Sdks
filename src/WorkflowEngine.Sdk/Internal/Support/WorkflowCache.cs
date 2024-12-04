using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Execution;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
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
        public int NumberOfActivityInstances => _summary.ActivityInstances.Count;

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
                FulcrumAssert.IsNotNull(_summary, CodeLocation.AsString());
                RememberData(_summary, true);

                // Check to see if an earlier execution failed to save to DB, but succeeded to save a fallback blob to storage
                await MaybeReadFromFallbackAndSaveToDbAsync(cancellationToken);
            }, cancellationToken);

            return;


        }

        private async Task MaybeReadFromFallbackAndSaveToDbAsync(CancellationToken cancellationToken)
        {
            if (_summary?.Instance == null) return;

            try
            {
                Log.LogInformation($"Looking for state fallback for {_summary?.Instance} ({_summary?.Instance?.Id}).");
                var summary = await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.ReadBlobAsync(_summary!.Instance!.Id, _summary!.Instance!.StartedAt, cancellationToken);
                if (summary == null) return;
                Log.LogInformation($"Found a state fallback for {summary?.Instance} ({summary?.Instance.Id}).");
                _summary = summary;
            }
            catch (FulcrumNotImplementedException)
            {
                // There was no fallback functionality
                return;
            }
            RememberData(_summary, false);
            // There was a fallback blob stored of the last state. Save that state to DB and remove the blob representation.
            Log.LogInformation($"Saving fallback state back to DB for {_summary?.Instance} ({_summary?.Instance?.Id}).");
            await SaveToDbOrFallbackAndThrowAsync(true, false, cancellationToken);
            Log.LogInformation($"Removing state fallback for {_summary?.Instance} ({_summary?.Instance?.Id}).");
            try
            {
                await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.DeleteBlobAsync(_summary!.Instance!.Id,
                    _summary.Instance.StartedAt, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to delete state fallback {_summary?.Instance} ({_summary?.Instance?.Id}). Will try again.:\r{ex.ToLog()}");
                throw new ActivityPostponedException(TimeSpan.FromSeconds(30));
            }
        }

        private async Task SaveToDbOrFallbackAndThrowAsync(bool hasSavedToFallback, bool doAnInitialSaveToFallback, CancellationToken cancellationToken)
        {
            // We will not let the database spend more than 30 seconds on saving.
            var limitedTimeCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitedTimeCancellationToken.Token);

            // Initial save to fallback?
            if (doAnInitialSaveToFallback)
            {
                try
                {
                    Log.LogWarning(
                        $"We will do an initial save to fallback storage for {_summary?.Instance} ({_summary?.Instance?.Id}).");
                    await SaveToFallbackAsync(cancellationToken);
                    hasSavedToFallback = true;
                }
                catch (Exception fallbackException)
                {
                    var message = $"The initial save to fallback storage failed for {_summary?.Instance} ({_summary?.Instance?.Id}). If we can't save the state to the DB we are in big trouble.:\r{fallbackException.ToLogString()}";
                    Log.LogError(message, fallbackException);
                }
            }

            // Save to DB and if fails: Save to fallback
            try
            {
                Log.LogInformation($"Saving state to DB for {_summary?.Instance} ({_summary?.Instance?.Id}).");
                await SaveToDbAsync(mergedToken.Token);
                Log.LogInformation($"Succeeded saving state fallback to DB for {_summary?.Instance} ({_summary?.Instance?.Id}).");
                if (doAnInitialSaveToFallback && hasSavedToFallback)
                {
                    await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.DeleteBlobAsync(_summary!.Instance!.Id, _summary.Instance.StartedAt, cancellationToken);
                }
            }
            catch (Exception dbException)
            {
                if (hasSavedToFallback) throw new ActivityPostponedException(TimeSpan.FromSeconds(30));
                try
                {
                    Log.LogWarning(
                        $"Failed to save to DB for {_summary?.Instance} ({_summary?.Instance?.Id}). Will try to save to secondary storage.:\r{dbException.ToLogString()}",
                        dbException);
                    await SaveToFallbackAsync(cancellationToken);
                }
                catch (Exception fallbackException)
                {
                    // This means that we have an execution that didn't succeed in saving its state, we must fail the entire workflow instance
                    var message =
                        $"The workflow instance {_summary?.Instance} ({_summary?.Instance?.Id}) could not save its state and will be cancelled on the next execution.:\r{fallbackException.ToLogString()}";
                    Log.LogError(message, fallbackException);
                }

                throw new ActivityPostponedException(TimeSpan.FromSeconds(30));
            }
        }

        public async Task<WorkflowSummary> SaveWithFallbackAsync(bool doAnInitialSaveToFallback,
            CancellationToken cancellationToken = default)
        {
            InternalContract.Require(_summary != null, $"The method {nameof(LoadAsync)} must be called before calling this method.");
            RememberData(_summary, false);
            return await _semaphore.ExecuteAsync(async ct =>
            {
                var oldForm = _workflowFormCache.GetStored(_summary.Form.Id);
                var oldVersion = _workflowVersionCache.GetStored(_summary.Version.Id);
                var oldInstance = _workflowInstanceCache.GetStored(_summary.Instance.Id);
                
                Log.LogInformation($"The workflow execution has finished, save state to DB for {_summary?.Instance} ({_summary?.Instance?.Id}).");
                await SaveToDbOrFallbackAndThrowAsync(false, doAnInitialSaveToFallback, cancellationToken);
                if (_workflowInformation.WorkflowOptions.AfterSaveAsync == null) return _summary;
                try
                {
                    await _workflowInformation.WorkflowOptions.AfterSaveAsync(
                        oldForm, oldVersion, oldInstance, _summary!.Form, _summary.Version, _summary.Instance);
                }
                catch (Exception e)
                {
                    // Fail handling is deferred to implementor. E.g., if using AM, the AM SDK will provide retry mechanism.
                    Log.LogError(
                        $"Error at {nameof(_stateCapability.WorkflowInstance.DefaultWorkflowOptions.AfterSaveAsync)}: {e.Message}. Giving up.", e);
                }
                return _summary;
            }, cancellationToken);
        }

        private void RememberData(WorkflowSummary summary, bool stored)
        {
            FulcrumAssert.IsNotNull(summary, CodeLocation.AsString());
            if (summary.Form != null)
            {
                _workflowFormCache.Add(summary.Form.Id, summary.Form, stored);
            }
            if (summary.Version != null)
            {
                _workflowVersionCache.Add(summary.Version.Id, summary.Version, stored);
            }
            if (summary.Instance != null)
            {
                _workflowInstanceCache.Add(summary.Instance.Id, summary.Instance, stored);
            }

            foreach (var activityForm in summary.ActivityForms.Values)
            {
                _activityFormCache.Add(activityForm.Id, activityForm, stored);
            }

            foreach (var activityVersion in summary.ActivityVersions.Values
                         .OrderBy(av => av, new ActivitySaveOrder()))
            {
                _activityVersionCache.Add(activityVersion.Id, activityVersion, stored);
            }

            foreach (var activityInstance in summary.ActivityInstances.Values
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

        public async Task SaveToDbAsync(CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(_summary, nameof(_summary));
            RememberData(_summary, false);

            // The following things are intentionally NOT in a transaction. This makes it possible for us to save them incrementally
            // over a number of executions. This is of importance when there are a lot of activity instances.
            try
            {
                Log.LogVerbose($"Saving workflow form for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _workflowFormCache.SaveAsync((_, item) => _summary.Form = item, cancellationToken);
                Log.LogVerbose($"Saving workflow version for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _workflowVersionCache.SaveAsync((_, item) => _summary.Version = item, cancellationToken);
                Log.LogVerbose($"Saving activity forms for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _activityFormCache.SaveAsync((id, item) => _summary.ActivityForms[id] = item, cancellationToken);
                Log.LogVerbose($"Saving activity versions for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _activityVersionCache.SaveAsync((id, item) => _summary.ActivityVersions[id] = item,
                    cancellationToken);
                Log.LogVerbose($"Saving activity instances for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _activityInstanceCache.SaveAsync((id, item) => _summary.ActivityInstances[id] = item,
                    cancellationToken);
                // Ths is intentionally put last in the save sequence; when we have succeed in saving this record,
                // it also marks that the complete state has been saved.
                Log.LogVerbose($"Saving workflow instance for {_summary.Instance} ({_summary.Instance?.Id}).");
                await _workflowInstanceCache.SaveAsync((_, item) => _summary.Instance = item, cancellationToken);
                Log.LogVerbose($"Done saving to DB for {_summary.Instance} ({_summary.Instance?.Id}).");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Save to DB failed for {_summary.Instance} ({_summary.Instance?.Id}):\r{ex.ToLog()}", ex);
                throw;
            }
        }

        private async Task SaveToFallbackAsync(CancellationToken cancellationToken)
        {
            if (_summary == null) return;
            // Remove unmodified items, so we only save the items that needs to be saved to the DB
            var summaryCopy = _summary.AsCopy();
            RemoveUnmodified(summaryCopy.ActivityForms, _activityFormCache, "activity forms");
            RemoveUnmodified(summaryCopy.ActivityVersions, _activityVersionCache, "activity versions");
            RemoveUnmodified(summaryCopy.ActivityInstances, _activityInstanceCache, "activity instances");
            var count = 0;
            while (true)
            {
                count++;
                try
                {
                    Log.LogInformation($"Try {count}. Try save to fallback for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}).");
                    // We must try at least once, so don't use a CancellationToken
                    // ReSharper disable once MethodSupportsCancellation
                    await _workflowCapabilities.StateCapability.WorkflowSummaryStorage.WriteBlobAsync(summaryCopy);
                    return;
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Try {count}. Could not save to fallback for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}):\r{ex.ToLog()}");
                    if (cancellationToken.IsCancellationRequested || ex is FulcrumNotImplementedException || FulcrumApplication.IsInDevelopment)
                    {
                        Log.LogError($"Try {count}. Gave up saving to fallback for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}).", ex);
                        throw;
                    }
                    // Ignore any cancellation token for this, we will try again. After the next try we will throw.
                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay(1000);
                    // Try again
                }
            }

            return;

            void RemoveUnmodified<TModelCreate, TModel>(IDictionary<string, TModel> itemDictionary,
                CrudPersistenceHelper<TModelCreate, TModel, string> cache, string itemNamePlural)
                where TModel : class, TModelCreate, IOptimisticConcurrencyControlByETag, IUniquelyIdentifiable<string>
            {
                Log.LogVerbose($"Contains {itemDictionary.Count} {itemNamePlural} before removing unmodified for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}).");
                
                var removed = 0;
                foreach (var (id, item) in itemDictionary.ToArray())
                {
                    var stored = cache.GetStored(id);
                    if (stored == null) continue;
                    if (item != null && !AreEqual(item, stored)) continue;
                    itemDictionary.Remove(id);
                    removed++;
                }
                Log.LogVerbose($"Removed {removed} unmodified {itemNamePlural} for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}).");
                Log.LogVerbose($"Contains {itemDictionary.Count} {itemNamePlural} after removing unmodified for workflow instance {summaryCopy.Instance} ({summaryCopy.Instance?.Id}).");
            }
        }

        private bool AreEqual<TModel>(TModel item, TModel stored)
            where TModel : class, IOptimisticConcurrencyControlByETag, IUniquelyIdentifiable<string>
        {
            if (item == null) return stored == null;
            if (stored == null) return false;
            var etag = item.Etag;
            item.Etag = stored.Etag;
            var equal = JToken.DeepEquals(JToken.FromObject(stored), JToken.FromObject(item));
            item.Etag = etag;
            return equal;
        }

        public ActivityForm GetActivityForm(string formId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            if (!_summary.ActivityForms.TryGetValue(formId, out var form)) return null;
            return form;
        }

        public ActivityVersion GetActivityVersion(string versionId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(versionId, nameof(versionId));
            if (!_summary.ActivityVersions.TryGetValue(versionId, out var version)) return null;
            return version;
        }

        public ActivityVersion GetActivityVersionByFormId(string formId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(formId, nameof(formId));
            return _summary.ActivityVersions.Values.FirstOrDefault(av => av.ActivityFormId == formId);
        }

        public ActivityInstance GetActivityInstance(string instanceId)
        {
            InternalContract.RequireNotNullOrWhiteSpace(instanceId, nameof(instanceId));
            if (!_summary.ActivityInstances.TryGetValue(instanceId, out var instance)) return null;
            return instance;
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