-- ROLLBACK: 7

CREATE TABLE Transition
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_Transition PRIMARY KEY NONCLUSTERED CONSTRAINT DF_Transition_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_Transition_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_Transition_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowVersionId uniqueidentifier NOT NULL CONSTRAINT FK_Transition_WorkflowVersionId REFERENCES WorkflowVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    FromActivityVersionId uniqueidentifier CONSTRAINT FK_Transition_FromActivityVersionId REFERENCES ActivityVersion ON UPDATE NO ACTION ON DELETE NO ACTION,
    ToActivityVersionId uniqueidentifier CONSTRAINT FK_Transition_ToActivityVersionId REFERENCES ActivityVersion ON UPDATE NO ACTION ON DELETE NO ACTION,

	CONSTRAINT UQ_Transition_1 UNIQUE (WorkflowVersionId, FromActivityVersionId, ToActivityVersionId)
)
CREATE CLUSTERED INDEX IX_Transition_RecordCreatedAt ON Transition (RecordCreatedAt)

CREATE TABLE WorkflowVersionParameter
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowVersionParameter PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowVersionParameter_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowVersionParameter_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowVersionParameter_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowVersionId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowVersionParameter_WorkflowVersionId REFERENCES WorkflowVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    Name nvarchar(2024) NOT NULl CONSTRAINT CK_WorkflowVersionParameter_Name_WS CHECK (ltrim(rtrim(Name)) != ''),

	CONSTRAINT UQ_WorkflowVersionParameter_1 UNIQUE (WorkflowVersionId, Name)
)
CREATE CLUSTERED INDEX IX_WorkflowVersionParameter_RecordCreatedAt ON WorkflowVersionParameter (RecordCreatedAt)

CREATE TABLE ActivityVersionParameter
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_ActivityVersionParameter PRIMARY KEY NONCLUSTERED CONSTRAINT DF_ActivityVersionParameter_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityVersionParameter_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityVersionParameter_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    ActivityVersionId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityVersionParameter_ActivityVersionId REFERENCES ActivityVersion ON UPDATE CASCADE ON DELETE NO ACTION,
    Name nvarchar(2024) NOT NULl CONSTRAINT CK_ActivityVersionParameter_Name_WS CHECK (ltrim(rtrim(Name)) != ''),

	CONSTRAINT UQ_ActivityVersionParameter_1 UNIQUE (ActivityVersionId, Name)
)
CREATE CLUSTERED INDEX IX_ActivityVersionParameter_RecordCreatedAt ON ActivityVersionParameter (RecordCreatedAt)
