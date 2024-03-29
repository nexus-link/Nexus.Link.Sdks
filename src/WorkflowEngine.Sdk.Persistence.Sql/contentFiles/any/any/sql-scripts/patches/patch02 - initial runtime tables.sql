﻿-- PATCH: 2

CREATE TABLE WorkflowInstance
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowInstance PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowInstance_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowVersionId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowInstance_WorkflowVersionId REFERENCES WorkflowVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    Title nvarchar(2048) NOT NULl CONSTRAINT CK_WorkflowInstance_Title_WS CHECK (ltrim(rtrim(Title)) != ''),
    InitialVersion nvarchar(64) NOT NULl CONSTRAINT CK_WorkflowInstanceInitialVersion_WS CHECK (ltrim(rtrim(InitialVersion)) != ''),
	StartedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_StartedAt DEFAULT (sysdatetimeoffset()),
	FinishedAt datetimeoffset,
	CancelledAt datetimeoffset,
    IsComplete bit NOT NULL  CONSTRAINT DF_WorkflowInstance_IsComplete DEFAULT ((1)),
    ResultAsJson nvarchar(MAX) NULL,
    ExceptionFriendlyMessage nvarchar(MAX) NULL,
    ExceptionTechnicalMessage nvarchar(MAX) NULL,
    [State] nvarchar(16)  NOT NULL  CONSTRAINT DF_WorkflowInstance_State DEFAULT ('Executing'), 
)
CREATE CLUSTERED INDEX IX_WorkflowInstance_RecordCreatedAt ON WorkflowInstance (RecordCreatedAt)

CREATE TABLE ActivityInstance
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_ActivityInstance PRIMARY KEY NONCLUSTERED CONSTRAINT DF_ActivityInstance_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityInstance_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityInstance_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowInstanceId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityInstance_WorkflowInstanceId REFERENCES WorkflowInstance ON UPDATE NO ACTION ON DELETE NO ACTION,
    ActivityVersionId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityInstance_ActivityVersionId REFERENCES ActivityVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    ParentActivityInstanceId uniqueidentifier CONSTRAINT FK_ActivityInstance_ParentActivityInstanceId REFERENCES ActivityInstance ON UPDATE NO ACTION ON DELETE NO ACTION,

    ParentIteration int,
	StartedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityInstance_StartedAt DEFAULT (sysdatetimeoffset()),
	FinishedAt datetimeoffset,
	ExceptionAlertHandled bit NULL,

    ResultAsJson nvarchar(max),
    [State] nvarchar(16) NOT NULL CONSTRAINT DF_ActivityInstance_State DEFAULT ('Executing'),
    ExceptionCategory nvarchar(16),
    ExceptionFriendlyMessage nvarchar(max),
    ExceptionTechnicalMessage nvarchar(max),
    AsyncRequestId nvarchar(64),

	
	CONSTRAINT UQ_ActivityInstance_1 UNIQUE (WorkflowInstanceId, ActivityVersionId, ParentActivityInstanceId, ParentIteration)
)
CREATE CLUSTERED INDEX IX_ActivityInstance_RecordCreatedAt ON ActivityInstance (RecordCreatedAt)

CREATE TABLE DistributedLock
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_DistributedLock PRIMARY KEY NONCLUSTERED CONSTRAINT DF_DistributedLock_Id DEFAULT (newid()),
	LockId uniqueidentifier NOT NULL,
	TableName nvarchar(50) NOT NULL,
	LockedRecordId uniqueidentifier NOT NULL,
	ValidUntil datetimeoffset NOT NULL,
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_DistributedLock_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_DistributedLock_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),
	
	CONSTRAINT UQ_DistributedLock_1 UNIQUE (LockedRecordId)
)
CREATE CLUSTERED INDEX IX_DistributedLock_RecordCreatedAt ON DistributedLock (RecordCreatedAt)
