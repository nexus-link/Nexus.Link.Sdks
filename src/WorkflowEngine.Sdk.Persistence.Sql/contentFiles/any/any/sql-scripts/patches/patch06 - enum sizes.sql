-- PATCH: 6

ALTER TABLE ActivityVersion ALTER COLUMN FailUrgency nvarchar(64)
ALTER TABLE WorkflowInstance ALTER COLUMN [State] nvarchar(64)
ALTER TABLE ActivityInstance ALTER COLUMN [State] nvarchar(64)
ALTER TABLE ActivityInstance ALTER COLUMN ExceptionCategory nvarchar(64)
