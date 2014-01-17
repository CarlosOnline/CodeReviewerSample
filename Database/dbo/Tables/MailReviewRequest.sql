CREATE TABLE [dbo].[MailReviewRequest] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [ReviewerAlias] NVARCHAR (50) NOT NULL,
    [ChangeListId]  INT           NOT NULL,
    CONSTRAINT [PK_MailReviewRequest] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MailReviewRequest_ChangeList] FOREIGN KEY ([ChangeListId]) REFERENCES [dbo].[ChangeList] ([Id])
);

