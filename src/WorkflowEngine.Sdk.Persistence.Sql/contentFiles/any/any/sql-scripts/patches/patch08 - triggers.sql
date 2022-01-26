-- PATCH: 8

ALTER TABLE WorkflowForm ADD CONSTRAINT UQ_WorkflowForm_1 UNIQUE (CapabilityName, Title)
GO

CREATE TRIGGER TR_WorkflowVersion_UnchangeableColumns ON WorkflowVersion
AFTER UPDATE
AS
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.MajorVersion != i.MajorVersion)
        THROW 100550, N'The value of the column "MajorVersion" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.WorkflowFormId != i.WorkflowFormId)
        THROW 100550, N'The value of the column "WorkflowFormId" is not allowed to be changed.',1
GO

CREATE TRIGGER TR_WorkflowInstance_UnchangeableColumns ON WorkflowInstance
AFTER UPDATE
AS
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.WorkflowVersionId != i.WorkflowVersionId)
        THROW 100550, N'The value of the column "WorkflowVersionId" is not allowed to be changed.',1
GO


CREATE TRIGGER TR_ActivityForm_UnchangeableColumns ON ActivityForm
AFTER UPDATE
AS
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.[Type] != i.[Type])
        THROW 100550, N'The value of the column "Type" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.WorkflowFormId != i.WorkflowFormId)
        THROW 100550, N'The value of the column "WorkflowFormId" is not allowed to be changed.',1
GO


CREATE TRIGGER TR_ActivityVersion_UnchangeableColumns ON ActivityVersion
AFTER UPDATE
AS
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.WorkflowVersionId != i.WorkflowVersionId)
        THROW 100550, N'The value of the column "WorkflowVersionId" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.ActivityFormId != i.ActivityFormId)
        THROW 100550, N'The value of the column "ActivityFormId" is not allowed to be changed.',1
GO


CREATE TRIGGER TR_ActivityInstance_UnchangeableColumns ON ActivityInstance
AFTER UPDATE
AS
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.StartedAt != i.StartedAt)
        THROW 100550, N'The value of the column "StartedAt" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.WorkflowInstanceId != i.WorkflowInstanceId)
        THROW 100550, N'The value of the column "WorkflowInstanceId" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.ActivityVersionId != i.ActivityVersionId)
        THROW 100550, N'The value of the column "ActivityVersionId" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.ParentActivityInstanceId != i.ParentActivityInstanceId
				OR d.ParentActivityInstanceId IS NULL AND i.ParentActivityInstanceId IS NOT NULL
				OR d.ParentActivityInstanceId IS NOT NULL AND i.ParentActivityInstanceId IS NULL)
        THROW 100550, N'The value of the column "ParentActivityInstanceId" is not allowed to be changed.',1
    IF EXISTS (
        SELECT 1
        FROM Inserted i
        JOIN Deleted d ON (i.id = d.id)
        WHERE d.ParentIteration != i.ParentIteration
				OR d.ParentIteration IS NULL AND i.ParentIteration IS NOT NULL
				OR d.ParentIteration IS NOT NULL AND i.ParentIteration IS NULL)
        THROW 100550, N'The value of the column "ParentIteration" is not allowed to be changed.',1
GO