CREATE TABLE [dbo].[ChangeList] (
    [Id]                    INT             IDENTITY (1, 1) NOT NULL,
    [SourceControlId]       INT             NOT NULL,
    [UserName]              NVARCHAR (200)  NOT NULL,
    [ReviewerAlias]         NVARCHAR (200)  NOT NULL,
    [UserClient]            NVARCHAR (50)   NOT NULL,
    [CL]                    NVARCHAR (128)  NOT NULL,
    [Url]                   NVARCHAR (2048) NOT NULL,
    [Title]                 NVARCHAR (128)  NOT NULL,
    [Description]           NVARCHAR (MAX)  NOT NULL,
    [TimeStamp]             DATETIME        NOT NULL,
    [Stage]                 INT             NOT NULL,
    [CurrentReviewRevision] INT             NOT NULL,
    CONSTRAINT [PK_ChangeList] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ChangeList_SourceControl] FOREIGN KEY ([SourceControlId]) REFERENCES [dbo].[SourceControl] ([Id])
);

