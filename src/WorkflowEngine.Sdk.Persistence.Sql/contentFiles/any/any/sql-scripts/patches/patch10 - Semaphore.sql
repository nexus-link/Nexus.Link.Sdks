-- PATCH: 10
CREATE TABLE [WorkflowSemaphore]
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowSemaphore PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowSemaphore_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowSemaphore_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowSemaphore_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowFormId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowSemaphore_WorkflowFormId REFERENCES WorkflowForm ON UPDATE CASCADE ON DELETE NO ACTION,
    ResourceIdentifier nvarchar(128) NOT NULL,
    Limit int NOT NULL CONSTRAINT CK_WorkflowSemaphore_Limit CHECK (Limit > 0),

	CONSTRAINT UQ_WorkflowSemaphore_1 UNIQUE (WorkflowFormId, ResourceIdentifier),	
	INDEX IX_WorkflowSemaphore_RecordCreatedAt CLUSTERED (RecordCreatedAt)
)

CREATE TABLE [WorkflowSemaphoreQueue]
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowSemaphoreQueue PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowSemaphoreQueue_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowSemaphoreQueue_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowSemaphoreQueue_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),
    
    WorkflowSemaphoreId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowSemaphoreQueue_WorkflowSemaphoreId REFERENCES WorkflowSemaphore ON UPDATE NO ACTION ON DELETE NO ACTION,
    WorkflowInstanceId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowSemaphoreQueue_WorkflowInstanceId REFERENCES WorkflowInstance ON UPDATE NO ACTION ON DELETE NO ACTION,
    Raised bit NOT NULL,
    ExpiresAt datetimeoffset NULL,
    ExpirationAfterSeconds float NOT NULL,

	CONSTRAINT UQ_WorkflowSemaphoreQueue_1 UNIQUE (WorkflowSemaphoreId, WorkflowInstanceId),
	INDEX IX_WorkflowSemaphoreQueue_RecordCreatedAt CLUSTERED (RecordCreatedAt)
)

