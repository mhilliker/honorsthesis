CREATE TABLE [dbo].[Table]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [URL] TEXT NULL, 
    [varName] NVARCHAR(50) NULL, 
    [min] REAL NULL DEFAULT 0, 
    [max] REAL NULL DEFAULT 0, 
    [numCases] INT NULL DEFAULT 1, 
    [numRuns] INT NULL DEFAULT 0
)
