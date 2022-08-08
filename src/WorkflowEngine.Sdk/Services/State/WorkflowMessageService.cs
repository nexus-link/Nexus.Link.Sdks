//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Nexus.Link.Capabilities.WorkflowConfiguration.Abstract.Entities;
//using Nexus.Link.Capabilities.WorkflowState.Abstract.Entities;
//using Nexus.Link.Capabilities.WorkflowState.Abstract.Messages;
//using Nexus.Link.Capabilities.WorkflowState.Abstract.Services;
//using Nexus.Link.Libraries.Core.Assert;
//using Nexus.Link.Libraries.Core.Queue.Logic;
//using Nexus.Link.Libraries.Core.Queue.Model;

//namespace Nexus.Link.WorkflowEngine.Sdk.Services.State;

///// <inheritdoc />
//public class WorkflowMessageService : IWorkflowMessageService
//{
//    private readonly IWritableQueue<WorkflowInstanceChangedV1> _workflowChangedEventQueue;
//    private readonly ILogService _logService;

//    private readonly MemoryQueue<WorkflowEventServiceQueueObject> _internalQueue;


//    /// <summary>
//    /// Constructor
//    /// </summary>
//    public WorkflowMessageService(string sourceClientId, IWritableQueue<WorkflowInstanceChangedV1> workflowChangedEventQueue, ILogService logService)
//    {
//        SourceClientId = sourceClientId;
//        _workflowChangedEventQueue = workflowChangedEventQueue;
//        _logService = logService;
//        _internalQueue = new MemoryQueue<WorkflowEventServiceQueueObject>($"{nameof(WorkflowMessageService)}RetryQueue");
//        _internalQueue.SetQueueItemAction(AddQueueMessage);
//    }

//    /// <inheritdoc />
//    public string SourceClientId { get; }

//    private async Task AddQueueMessage(WorkflowEventServiceQueueObject item, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (item.EventObject is WorkflowInstanceChangedV1 @event)
//            {
//                await _workflowChangedEventQueue.AddMessageAsync(@event, null, cancellationToken);
//            }
//        }
//        catch (Exception e)
//        {
//            if (item.Retries >= 5)
//            {
//                // Log and give up
//                // TODO: Log
//                //await _logService.CreateAsync(new LogCreate
//                //{
//                //    WorkflowFormId = 
//                //}, cancellationToken);
//                return;
//            }

//            // Try again on internal queue
//            item.Retries++;
//            await _internalQueue.AddMessageAsync(item, cancellationToken: cancellationToken);
//        }
//    }

//    ///// <inheritdoc />
//    //public async Task FireWorkflowInstanceChangedAsync(WorkflowInstanceChangedV1 @event, CancellationToken cancellationToken = default)
//    //{
//    //    InternalContract.RequireNotNull(@event, nameof(@event));
//    //    InternalContract.RequireValidated(@event, nameof(@event));

//    //    await _internalQueue.AddMessageAsync(new WorkflowEventServiceQueueObject { EventObject = @event }, cancellationToken: cancellationToken);
//    //}

//    ///// <inheritdoc />
//    //public async Task FireWorkflowInstanceChangedAsync(string workflowInstanceId, DateTimeOffset changedAt, CancellationToken cancellationToken = default)
//    //{
//    //    InternalContract.RequireNotNullOrWhiteSpace(workflowInstanceId, nameof(workflowInstanceId));
//    //    InternalContract.RequireNotDefaultValue(changedAt, nameof(changedAt));

//    //    var message = new WorkflowEventServiceQueueObject
//    //    {
//    //        EventObject = new WorkflowInstanceChangedV1
//    //        {
//    //            Payload = new()
//    //            {
//    //                SourceClientId = SourceClientId,
//    //                WorkflowInstanceId = workflowInstanceId,
//    //                ChangedAt = changedAt
//    //            },
//    //            Timestamp = DateTimeOffset.Now
//    //        }
//    //    };
//    //    await _internalQueue.AddMessageAsync(message, cancellationToken: cancellationToken);
//    //}

//    private class WorkflowEventServiceQueueObject
//    {
//        public object EventObject { get; set; }
//        public int Retries { get; set; }
//    }

//    /// <inheritdoc />
//    public async Task PublishWorkflowInstanceChangedMessageAsync(WorkflowForm form, WorkflowVersion version, WorkflowInstance instance, CancellationToken cancellationToken = default)
//    {
//        InternalContract.RequireNotNull(form, nameof(form));
//        InternalContract.RequireNotNull(version, nameof(version));
//        InternalContract.RequireNotNull(instance, nameof(instance));

//        var message = new WorkflowEventServiceQueueObject
//        {
//            EventObject = new WorkflowInstanceChangedV1
//            {
//                SourceClientId = SourceClientId,
//                Form = form,
//                Version = version,
//                Instance = instance,
//                Timestamp = DateTimeOffset.Now
//            }
//        };
//        await _internalQueue.AddMessageAsync(message, cancellationToken: cancellationToken);
//    }
//}