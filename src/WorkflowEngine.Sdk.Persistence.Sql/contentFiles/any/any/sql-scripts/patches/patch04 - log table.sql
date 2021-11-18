-- PATCH: 4

CREATE TABLE [Log]
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_Log PRIMARY KEY NONCLUSTERED CONSTRAINT DF_Log_Id DEFAULT (newid()),
    WorkflowFormId uniqueidentifier NOT NULL,
    WorkflowInstanceId uniqueidentifier NULL,
    ActivityFormId uniqueidentifier NULL,
	SeverityLevel nvarchar(16) NOT NULL,
	[Message] nvarchar(Max) NOT NULL,
	DataAsJson nvarchar (max) NULL,
	[TimeStamp] datetimeoffset NOT NULL CONSTRAINT DF_Log_TimeStamp DEFAULT (sysdatetimeoffset()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_Log_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_Log_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),
	
	INDEX IX_Log_RecordCreatedAt CLUSTERED (RecordCreatedAt),
	INDEX IX_Log_WorkflowFormId (WorkflowFormId),
	INDEX IX_Log_WorkflowInstanceId (WorkflowInstanceId),
	INDEX IX_Log_ActivityFormId (ActivityFormId)
)