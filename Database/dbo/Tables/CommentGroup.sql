CREATE TABLE [dbo].[CommentGroup] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [ReviewId]      INT            NOT NULL,
    [ChangeListId]  INT            NOT NULL,
    [FileId]        INT            NOT NULL,
    [FileVersionId] INT            NOT NULL,
    [Line]          INT            NOT NULL,
    [LineStamp]     NVARCHAR (200) NOT NULL,
    [Status]        INT            NOT NULL,
    CONSTRAINT [PK_CommentGroup] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CommentGroup_ChangeFile] FOREIGN KEY ([FileId]) REFERENCES [dbo].[ChangeFile] ([Id]),
    CONSTRAINT [FK_CommentGroup_FileVersion] FOREIGN KEY ([FileVersionId]) REFERENCES [dbo].[FileVersion] ([Id]),
    CONSTRAINT [FK_CommentGroup_Review] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[Review] ([Id]),
    UNIQUE NONCLUSTERED ([LineStamp] ASC)
);

