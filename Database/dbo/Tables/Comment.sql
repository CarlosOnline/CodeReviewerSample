CREATE TABLE [dbo].[Comment] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [CommentText]    NVARCHAR (2048) NOT NULL,
    [FileVersionId]  INT             NOT NULL,
    [ReviewerId]     INT             NOT NULL,
    [ReviewRevision] INT             NOT NULL,
    [GroupId]        INT             NULL,
    [UserName]       NVARCHAR (200)  NOT NULL,
    [ReviewerAlias]  NVARCHAR (200)  NOT NULL,
    CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Comment_CommentGroup] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[CommentGroup] ([Id]),
    CONSTRAINT [FK_Comment_FileVersion] FOREIGN KEY ([FileVersionId]) REFERENCES [dbo].[FileVersion] ([Id]),
    CONSTRAINT [FK_Comment_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Reviewer] ([Id])
);

