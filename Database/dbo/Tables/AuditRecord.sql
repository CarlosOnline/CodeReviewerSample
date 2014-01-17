CREATE TABLE [dbo].[AuditRecord] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [TimeStamp]    DATETIME       NOT NULL,
    [UserName]     NVARCHAR (200) NOT NULL,
    [ChangeListId] INT            NOT NULL,
    [Action]       NVARCHAR (50)  NOT NULL,
    [Description]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AuditRecord] PRIMARY KEY CLUSTERED ([Id] ASC)
);

