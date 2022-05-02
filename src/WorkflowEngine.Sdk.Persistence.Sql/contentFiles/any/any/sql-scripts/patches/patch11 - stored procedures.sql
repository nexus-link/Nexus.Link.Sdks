-- PATCH: 11
CREATE PROCEDURE DeleteWorkflowVersion (@workflowVersionId UNIQUEIDENTIFIER OUTPUT) AS
BEGIN
	DELETE FROM l
		FROM WorkflowInstance wi
		JOIN [Log] l ON (l.WorkflowInstanceId = wi.Id)
		WHERE wi.WorkflowVersionId = @workflowVersionId

	DELETE FROM dl
		FROM WorkflowInstance wi
		JOIN DistributedLock dl ON (dl.LockedRecordId = wi.Id)
		WHERE wi.WorkflowVersionId = @workflowVersionId

	DELETE FROM ai
		FROM  WorkflowInstance wi
		JOIN ActivityInstance ai ON (ai.WorkflowInstanceId = wi.Id)
		WHERE wi.WorkflowVersionId = @workflowVersionId
	
	DELETE FROM ActivityVersion
		WHERE WorkflowVersionId = @workflowVersionId

	DELETE FROM wsq
		FROM  WorkflowInstance wi
		JOIN WorkflowSemaphoreQueue wsq ON (wsq.WorkflowInstanceId = wi.Id)
		WHERE wi.WorkflowVersionId = @workflowVersionId
	
	DELETE FROM WorkflowInstance
		WHERE WorkflowVersionId = @workflowVersionId

	DELETE FROM WorkflowVersion
		WHERE Id = @workflowVersionId
END
GO

CREATE PROCEDURE DeleteWorkflowForm (@workflowFormId UNIQUEIDENTIFIER OUTPUT) AS
BEGIN
	DELETE FROM l
		FROM WorkflowVersion wv
		JOIN WorkflowInstance wi ON (wi.WorkflowVersionId = wv.Id)
		JOIN [Log] l ON (l.WorkflowInstanceId = wi.Id)
		WHERE wv.WorkflowFormId = @workflowFormId

	DELETE FROM dl
		FROM WorkflowVersion wv
		JOIN WorkflowInstance wi ON (wi.WorkflowVersionId = wv.Id)
		JOIN DistributedLock dl ON (dl.LockedRecordId = wi.Id)
		WHERE wv.WorkflowFormId = @workflowFormId

	DELETE FROM ai
		FROM WorkflowVersion wv
		JOIN WorkflowInstance wi ON (wi.WorkflowVersionId = wv.Id)
		JOIN ActivityInstance ai ON (ai.WorkflowInstanceId = wi.Id)
		WHERE wv.WorkflowFormId = @workflowFormId
	
	DELETE FROM av
		FROM WorkflowVersion wv
		JOIN ActivityVersion av ON (av.WorkflowVersionId = wv.Id)
		WHERE wv.WorkflowFormId = @workflowFormId
	
	DELETE FROM ActivityForm
		WHERE WorkflowFormId = @workflowFormId

	DELETE FROM wsq
		FROM WorkflowVersion wv
		JOIN WorkflowInstance wi ON (wi.WorkflowVersionId = wv.Id)
		JOIN WorkflowSemaphoreQueue wsq ON (wsq.WorkflowInstanceId = wi.Id)
		WHERE wv.WorkflowFormId = @workflowFormId

	DELETE FROM WorkflowSemaphore
		WHERE WorkflowFormId = @workflowFormId
	
	DELETE FROM wi
		FROM WorkflowVersion wv
		JOIN WorkflowInstance wi ON (wi.WorkflowVersionId = wv.Id)
		WHERE wv.WorkflowFormId = @workflowFormId

	DELETE FROM WorkflowVersion
		WHERE WorkflowFormId = @workflowFormId

	DELETE FROM WorkflowForm
		WHERE Id = @workflowFormId
END
GO