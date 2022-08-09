-- PATCH: 15

ALTER TABLE [WorkflowInstance] ADD ExecutionId nvarchar(64) NULL
GO

CREATE UNIQUE NONCLUSTERED INDEX UX_WorkflowInstance_ExecutionId ON dbo.WorkflowInstance
	(
	ExecutionId
	) WHERE ExecutionId IS NOT NULL