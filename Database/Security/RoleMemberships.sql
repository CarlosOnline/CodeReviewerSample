ALTER ROLE [db_owner] ADD MEMBER [CodeReviewUser];
go
exec sp_addrolemember 'db_owner', [CodeReviewUser];
go
