-- PATCH: 9
CREATE TABLE [HashRecord]
(
    Id uniqueidentifier NOT NULL ROWGUIDCOL CONSTRAINT PK_ReentryToken PRIMARY KEY NONCLUSTERED CONSTRAINT DF_ReentryToken_Id DEFAULT (newid()),
	[Hash] nvarchar(MAX) NOT NULL,
	Salt nvarchar (MAX) NOT NULL,
    RecordVersion rowversion NOT NULL,
    RecordCreatedAt datetimeoffset NOT NULL CONSTRAINT DF_ReentryToken_RecordCreatedAt DEFAULT (sysdatetimeoffset()),
	RecordUpdatedAt datetimeoffset NOT NULL CONSTRAINT DF_ReentryToken_RecordUpdatedAt DEFAULT (sysdatetimeoffset()),
	
	INDEX IX_ReentryToken_RecordCreatedAt CLUSTERED (RecordCreatedAt)
)

