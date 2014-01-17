CREATE TABLE [dbo].[MailReview] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [ReviewId] INT NOT NULL,
    CONSTRAINT [PK_MailReview] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MailReview_Review] FOREIGN KEY ([ReviewId]) REFERENCES [dbo].[Review] ([Id])
);

