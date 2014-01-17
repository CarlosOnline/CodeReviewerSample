CREATE TABLE [dbo].[MailChangeList] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [ReviewerId]   INT NOT NULL,
    [ChangeListId] INT NOT NULL,
    [RequestType]  INT NOT NULL,
    CONSTRAINT [PK_MailChangeList] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MailChangeList_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id]),
    CONSTRAINT [FK_MailChangeList_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Reviewer] ([Id])
);

