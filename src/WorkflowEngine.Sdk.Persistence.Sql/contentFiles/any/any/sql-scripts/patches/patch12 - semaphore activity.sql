-- PATCH: 12

ALTER TABLE [WorkflowSemaphore] ALTER COLUMN WorkflowFormId uniqueidentifier NULL
ALTER TABLE [WorkflowSemaphoreQueue] ADD ParentActivityInstanceId uniqueidentifier NULL
ALTER TABLE [WorkflowSemaphoreQueue] ADD ParentIteration integer NULL

ALTER TABLE [WorkflowSemaphoreQueue] DROP CONSTRAINT UQ_WorkflowSemaphoreQueue_1
ALTER TABLE [WorkflowSemaphoreQueue] ADD CONSTRAINT UQ_WorkflowSemaphoreQueue_1 UNIQUE (WorkflowSemaphoreId, WorkflowInstanceId, ParentActivityInstanceId, ParentIteration)
