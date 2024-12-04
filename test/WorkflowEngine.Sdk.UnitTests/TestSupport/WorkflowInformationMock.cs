using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Configuration.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Exceptions;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Entities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Support;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Interfaces;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;
using Activity = Nexus.Link.WorkflowEngine.Sdk.Internal.Logic.Activity;

namespace WorkflowEngine.Sdk.UnitTests.TestSupport;

internal class WorkflowInformationMock : IWorkflowInformation
{
    private ActivityInstance _activityInstance;
    private ActivityVersion _activityVersion;
    private ActivityForm _activityForm;

    public WorkflowInformationMock(IActivityExecutor activityExecutor, ILogicExecutor logicExecutor, CancellationToken reducedTimeCancellationToken = default)
    {
        TimeSinceCurrentRunStarted = new Stopwatch();
        TimeSinceCurrentRunStarted.Start();
        ActivityExecutor = activityExecutor;
        LogicExecutor = logicExecutor;
        ReducedTimeCancellationToken = reducedTimeCancellationToken;
    }

    public WorkflowInformationMock(WorkflowForm form, WorkflowVersion version, WorkflowInstance instance)
    {
        Form = form;
        Version = version;
        Instance = instance;
        TimeSinceCurrentRunStarted = new Stopwatch();
        TimeSinceCurrentRunStarted.Start();
    }

    public IActivityExecutor ActivityExecutor { get; set; }
    public ILogicExecutor LogicExecutor { get; set; }

    public IMethodMock MethodMock { get; set; }

    /// <inheritdoc />
    public string CapabilityName => Form.CapabilityName;

    /// <inheritdoc />
    public string FormId => Form?.Id;

    /// <inheritdoc />
    public string FormTitle => Form?.Title;

    /// <inheritdoc />
    public WorkflowForm Form { get; set; } = new()
    {
        Id = "1BF92C6F-CB6E-44B3-B9AF-1FA9E15DC732",
        CapabilityName = "Capability name",
        Title = "Form title",
    };

    /// <inheritdoc />
    public int MajorVersion => Version.MajorVersion;

    /// <inheritdoc />
    public int MinorVersion => Version.MinorVersion;

    /// <inheritdoc />
    public WorkflowVersion Version { get; set; } = new()
    {
        MajorVersion = 1,
        MinorVersion = 0,
    };

    /// <inheritdoc />
    public string InstanceId
    {
        get => Instance?.Id ?? Guid.NewGuid().ToString();
        set => Instance.Id = value;
    }

    /// <inheritdoc />
    public WorkflowInstance Instance { get; set; } = new()
    {
        Id = "44286249-FDDE-40AD-860C-89F49FF92792",
        Title = "Instance title",
        StartedAt = DateTimeOffset.Now
    };

    /// <inheritdoc />
    public string InstanceTitle => Instance.Title;

    /// <inheritdoc />
    public DateTimeOffset StartedAt => Instance.StartedAt;

    /// <inheritdoc />
    public ILogService LogService { get; set; } = null;

    /// <inheritdoc />
    public ICollection<LogCreate> Logs { get; set; } = new List<LogCreate>();

    /// <inheritdoc />
    public IWorkflowSemaphoreService SemaphoreService { get; set; } = null;

    /// <inheritdoc />
    public IWorkflowInstanceService WorkflowInstanceService { get; set; } = null;

    /// <inheritdoc />
    public ActivityOptions DefaultActivityOptions { get; set; }

    /// <inheritdoc />
    public IInternalActivity LatestActivity { get; set; }

    /// <inheritdoc />
    public ActivityDefinition GetActivityDefinition(string activityFormId, string title)
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
        if (MethodMock != null) return MethodMock.GetActivityInstance(activityInstanceId);

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
    public int NumberOfActivityInstances => 1;

    /// <inheritdoc />
    public void AddActivity(IInternalActivity activity)
    {
    }

    /// <inheritdoc />
    public string GetOrCreateActivityInstanceId(IActivityInformation activityInformation)
    {
        return "1EA54949-94C6-469E-857F-E16EC216D498";
    }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task SaveAsync(bool hasSavedToFallback, bool doAnInitialSaveToFallback, CancellationToken cancellationToken)
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

    /// <inheritdoc />
    public IActivityExecutor GetActivityExecutor(Activity activity) => ActivityExecutor;

    /// <inheritdoc />
    public ILogicExecutor GetLogicExecutor(IInternalActivity activity) => LogicExecutor;

    /// <inheritdoc />
    public bool TryGetActivity(string activityId, out Activity activity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool TryGetActivity<TActivityReturns>(string activityId, out Activity<TActivityReturns> activity)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public TActivityReturns GetActivityResult<TActivityReturns>(string activityInstanceId)
    {
        // https://stackoverflow.com/questions/5780888/casting-interfaces-for-deserialization-in-json-net
        var instance = GetActivityInstance(activityInstanceId);
        FulcrumAssert.IsNotNull(instance, CodeLocation.AsString());
        FulcrumAssert.IsTrue(instance.HasCompleted, CodeLocation.AsString());
        if (instance.State == ActivityStateEnum.Success)
        {
            FulcrumAssert.IsNotNull(instance.ResultAsJson);
            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<TActivityReturns>(instance.ResultAsJson);
                return deserializedObject;
            }
            catch (Exception e)
            {
                throw new FulcrumAssertionFailedException(
                    $"Could not deserialize activity {this} to type {typeof(TActivityReturns).Name}:{e}\r{instance.ResultAsJson}");
            }
        }
        FulcrumAssert.IsNotNull(instance.ExceptionCategory, CodeLocation.AsString());
        throw new ActivityFailedException(instance.ExceptionCategory!.Value, instance.ExceptionTechnicalMessage,
            instance.ExceptionFriendlyMessage);
    }

    /// <inheritdoc />
    public CancellationToken ReducedTimeCancellationToken { get; set; }

    /// <inheritdoc />
    public Stopwatch TimeSinceCurrentRunStarted { get; }

    public WorkflowOptions WorkflowOptions { get; set; } = new();

    /// <inheritdoc />
    public NexusAsyncSemaphore ReadResponsesSemaphore { get; } = new NexusAsyncSemaphore(1);

    /// <inheritdoc />
    public IDictionary<string, HttpResponse> HttpAsyncResponses { get; set; }

    /// <inheritdoc />
    public string ToLogString()
    {
        string title;
        try
        {
            title = InstanceTitle;
        }
        catch (Exception)
        {
            title = FormTitle;
        }

        var id = !string.IsNullOrWhiteSpace(InstanceId) ? $"instance id: {InstanceId}" : $"form id: {FormId}";
        var state = Instance == null ? "" : Instance.State.ToString();
        return $"{title}{state} (id)";
    }
}