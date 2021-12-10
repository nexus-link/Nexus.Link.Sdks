-- ROLLBACK: 6


ALTER TABLE ActivityInstance ALTER COLUMN ExceptionCategory nvarchar(16)
ALTER TABLE ActivityInstance ALTER COLUMN [State] nvarchar(16)
ALTER TABLE WorkflowInstance ALTER COLUMN [State] nvarchar(16)
ALTER TABLE ActivityVersion ALTER COLUMN FailUrgency nvarchar(16)
