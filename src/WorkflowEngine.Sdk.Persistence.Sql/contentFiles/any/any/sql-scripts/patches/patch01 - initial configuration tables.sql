﻿-- PATCH: 1

----------------------------------------------------------------------------------------------------------------------
-- Workflow form definition
----------------------------------------------------------------------------------------------------------------------

CREATE TABLE WorkflowForm
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowForm PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowForm_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowForm_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowForm_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    CapabilityName nvarchar(2024) NOT NULL CONSTRAINT CK_WorkflowForm_CapabilityName_WS CHECK (ltrim(rtrim(CapabilityName)) != ''),
    Title nvarchar(2024) NOT NULl CONSTRAINT CK_WorkflowForm_Title_WS CHECK (ltrim(rtrim(Title)) != '')
)
CREATE CLUSTERED INDEX IX_WorkflowForm_RecordCreatedAt ON WorkflowForm (RecordCreatedAt)

CREATE TABLE WorkflowVersion
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_WorkflowVersion PRIMARY KEY NONCLUSTERED CONSTRAINT DF_WorkflowVersion_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowVersion_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_WorkflowVersion_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowFormId uniqueidentifier NOT NULL CONSTRAINT FK_WorkflowVersion_WorkflowFormId REFERENCES WorkflowForm ON UPDATE CASCADE ON DELETE NO ACTION,
    MajorVersion int NOT NULL CONSTRAINT CK_WorkflowVersion_MajorVersion CHECK (MajorVersion > 0),
    MinorVersion int NOT NULL CONSTRAINT CK_WorkflowVersion_MinorVersion CHECK (MinorVersion >= 0),
    DynamicCreate bit NOT NULL

	CONSTRAINT UQ_WorkflowVersion_1 UNIQUE (WorkflowFormId, MajorVersion)
)
CREATE CLUSTERED INDEX IX_WorkflowVersion_RecordCreatedAt ON WorkflowVersion (RecordCreatedAt)

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


----------------------------------------------------------------------------------------------------------------------
-- Activity form definition
----------------------------------------------------------------------------------------------------------------------

CREATE TABLE ActivityForm
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_ActivityForm PRIMARY KEY NONCLUSTERED CONSTRAINT DF_ActivityForm_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityForm_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityForm_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowFormId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityForm_WorkflowFormId REFERENCES WorkflowForm ON UPDATE CASCADE ON DELETE NO ACTION,
    Type nvarchar(64) NOT NULL CONSTRAINT CK_ActivityForm_Type_WS CHECK (ltrim(rtrim(Type)) != ''),
    Title nvarchar(2048) NOT NULl CONSTRAINT CK_ActivityForm_Title_WS CHECK (ltrim(rtrim(Title)) != '')
)
CREATE CLUSTERED INDEX IX_ActivityForm_RecordCreatedAt ON ActivityForm (RecordCreatedAt)

CREATE TABLE ActivityVersion
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_ActivityVersion PRIMARY KEY NONCLUSTERED CONSTRAINT DF_ActivityVersion_Id DEFAULT (newid()),
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityVersion_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ActivityVersion_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),

    WorkflowVersionId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityVersion_WorkflowVersionId REFERENCES WorkflowVersion ON UPDATE NO ACTION ON DELETE NO ACTION,
    ActivityFormId uniqueidentifier NOT NULL CONSTRAINT FK_ActivityVersion_ActivityFormId REFERENCES ActivityForm ON UPDATE CASCADE ON DELETE NO ACTION,
    Position int NOT NULL CONSTRAINT CK_ActivityVersion_Position_GT0 CHECK (Position >= 1),
    ParentActivityVersionId uniqueidentifier CONSTRAINT FK_ActivityVersion_ParentActivityVersionId REFERENCES ActivityVersion ON UPDATE NO ACTION ON DELETE NO ACTION,
    FailUrgency nvarchar(16) NOT NULL CONSTRAINT DF_ActivityVersion_FailUrgency DEFAULT ('Stopping'),

	CONSTRAINT UQ_ActivityVersion_1 UNIQUE (WorkflowVersionId, ActivityFormId)
)
CREATE CLUSTERED INDEX IX_ActivityVersion_RecordCreatedAt ON ActivityVersion (RecordCreatedAt)

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

