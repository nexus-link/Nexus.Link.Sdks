﻿-- PATCH: 2

CREATE TABLE WorkflowInstance
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT DF_WorkflowInstance_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowVersionId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowInstance_WorkflowVersionId REFERENCES WorkflowVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    Title nvarchar(2048) NOT NULl CONSTRAINT CK_WorkflowInstance_Title_WS CHECK (ltrim(rtrim(Title)) != ''),
    InitialVersion nvarchar(64) NOT NULl CONSTRAINT CK_WorkflowInstanceInitialVersion_WS CHECK (ltrim(rtrim(InitialVersion)) != ''),
	StartedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowInstance_StartedAt DEFAULT (sysdatetimeoffset()),
	FinishedAt datetimeoffset CONSTRAINT DF_WorkflowInstance_FinishedAt DEFAULT (sysdatetimeoffset()),

	CONSTRAINT PK_WorkflowInstance PRIMARY KEY (Id ASC)
)

CREATE TABLE ActivityInstance
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT DF_ActivityInstance_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityInstance_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityInstance_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowInstanceId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityInstance_WorkflowInstanceId REFERENCES WorkflowInstance ON UPDATE NO ACTION ON DELETE NO ACTION,
    ActivityVersionId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityInstance_ActivityVersionId REFERENCES ActivityVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    ParentActivityInstanceId uniqueidentifier CONSTRAINT FK_ActivityInstance_ParentActivityInstanceId REFERENCES ActivityVersion ON UPDATE NO ACTION ON DELETE NO ACTION,

    ParentIteration int,
	StartedAt datetimeoffset NOT NULL CONSTRAINT DFActivityInstance_StartedAt DEFAULT (sysdatetimeoffset()),
	FinishedAt datetimeoffset CONSTRAINT DF_ActivityInstance_FinishedAt DEFAULT (sysdatetimeoffset()),
	HasCompleted bit CONSTRAINT DF_ActivityInstance_HasCompleted DEFAULT (0),

    ResultAsJson nvarchar(max),
    ExceptionName nvarchar(2048),
    ExceptionMessage nvarchar(max),
    AsyncRequestId nvarchar(256),

	CONSTRAINT PK_ActivityInstance PRIMARY KEY (Id ASC)
    -- TODO: Unique index on (WorkflowInstanceId, ActivityVersionId, ParentActivityInstanceId, Iteration)?
)

