-- PATCH: 2

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
	HasCompleted bit CONSTRAINT DF_ActivityInstance_HasCompleted DEFAULT (0),

    ResultAsJson nvarchar(max),
    State nvarchar(16) NOT NULL CONSTRAINT DF_ActivityInstance_State DEFAULT ('Started'),
    ExceptionCategory nvarchar(16),
    FailUrgency nvarchar(16) NOT NULL CONSTRAINT DF_ActivityInstance_FailUrgency DEFAULT ('Stopping'),
    ExceptionFriendlyMessage nvarchar(max),
    ExceptionTechnicalMessage nvarchar(max),
    AsyncRequestId nvarchar(64),

	
	CONSTRAINT UQ_ActivityInstance_1 UNIQUE (WorkflowInstanceId, ActivityVersionId, ParentActivityInstanceId, ParentIteration)
)
CREATE CLUSTERED INDEX IX_ActivityInstance_RecordCreatedAt ON ActivityInstance (RecordCreatedAt)

