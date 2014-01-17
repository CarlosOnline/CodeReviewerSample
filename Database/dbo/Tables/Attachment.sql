CREATE TABLE [dbo].[Attachment] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [ChangeListId] INT            NOT NULL,
    [TimeStamp]    DATETIME       NOT NULL,
    [Description]  NVARCHAR (128) NULL,
    [Link]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Attachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Attachment_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id])
);

