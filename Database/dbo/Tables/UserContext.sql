CREATE TABLE [dbo].[UserContext] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [UserName]      NVARCHAR (200) NOT NULL,
    [ReviewerAlias] NVARCHAR (200) NOT NULL,
    [KeyName]       NVARCHAR (50)  NOT NULL,
    [Value]         NVARCHAR (MAX) NULL,
    [Version]       INT            NOT NULL,
    CONSTRAINT [PK_UserContext] PRIMARY KEY CLUSTERED ([Id] ASC)
);

