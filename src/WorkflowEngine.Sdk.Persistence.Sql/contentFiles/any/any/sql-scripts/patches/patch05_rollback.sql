-- ROLLBACK: 5

ALTER TABLE [Log] DROP CONSTRAINT DF_Log_SeverityLevelNumber
ALTER TABLE [Log] DROP COLUMN SeverityLevelNumber

