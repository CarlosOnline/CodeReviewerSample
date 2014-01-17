CREATE TABLE [dbo].[Reviewer] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [UserName]      NVARCHAR (200) NOT NULL,
    [ReviewerAlias] NVARCHAR (200) NOT NULL,
    [ChangeListId]  INT            NOT NULL,
    [Status]        INT            NOT NULL,
    CONSTRAINT [PK_Reviewer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Reviewer_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id]),
    UNIQUE NONCLUSTERED ([ChangeListId] ASC, [UserName] ASC, [ReviewerAlias] ASC)
);

