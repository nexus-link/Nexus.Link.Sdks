-- PATCH: 5

ALTER TABLE [Log] ADD SeverityLevelNumber int NOT NULL CONSTRAINT DF_Log_SeverityLevelNumber DEFAULT (1)

