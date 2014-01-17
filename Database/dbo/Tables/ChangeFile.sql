CREATE TABLE [dbo].[ChangeFile] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [ChangeListId]   INT            NOT NULL,
    [LocalFileName]  NVARCHAR (512) NOT NULL,
    [ServerFileName] NVARCHAR (512) NOT NULL,
    [IsActive]       BIT            NOT NULL,
    [ReviewRevision] INT            NOT NULL,
    CONSTRAINT [PK_ChangeFile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChangeFile_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id])
);

