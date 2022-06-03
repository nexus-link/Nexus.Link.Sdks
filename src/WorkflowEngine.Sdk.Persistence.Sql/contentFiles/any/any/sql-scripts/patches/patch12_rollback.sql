-- ROLLBACK: 12

ALTER TABLE [WorkflowSemaphoreQueue] DROP ParentActivityInstanceId
ALTER TABLE [WorkflowSemaphoreQueue] DROP ParentIteration

ALTER TABLE [WorkflowSemaphoreQueue] DROP CONSTRAINT UQ_WorkflowSemaphoreQueue_1
ALTER TABLE [WorkflowSemaphoreQueue] ADD CONSTRAINT UQ_WorkflowSemaphoreQueue_1 UNIQUE (WorkflowSemaphoreId, WorkflowInstanceId, ParentActivityInstanceId, ParentIteration)