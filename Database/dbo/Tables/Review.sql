CREATE TABLE [dbo].[Review] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [ChangeListId]  INT             NOT NULL,
    [UserName]      NVARCHAR (200)  NOT NULL,
    [ReviewerAlias] NVARCHAR (200)  NOT NULL,
    [TimeStamp]     DATETIME        NOT NULL,
    [IsSubmitted]   BIT             NOT NULL,
    [OverallStatus] TINYINT         NOT NULL,
    [CommentText]   NVARCHAR (2048) NULL,
    CONSTRAINT [PK_Review] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Review_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id])
);

