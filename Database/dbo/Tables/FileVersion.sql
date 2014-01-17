CREATE TABLE [dbo].[FileVersion] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [FileId]         INT           NOT NULL,
    [Revision]       INT           NOT NULL,
    [ReviewRevision] INT           NOT NULL,
    [Action]         INT           NOT NULL,
    [TimeStamp]      DATETIME      NULL,
    [IsText]         BIT           NOT NULL,
    [IsFullText]     BIT           NOT NULL,
    [IsRevisionBase] BIT           NOT NULL,
    [Text]           VARCHAR (MAX) NULL,
    CONSTRAINT [PK_FileVersion] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_FileVersion_ChangeFile] FOREIGN KEY ([FileId]) REFERENCES [dbo].[ChangeFile] ([Id])
);

