--alter login [CodeReviewUser] with default_database = [CodeReviewer];
--go
--alter authorization on database::[CodeReviewer] to [CodeReviewUser];
--go
INSERT INTO SourceControl([Type], [Server], [Client], [Description], [WebsiteName])
VALUES(2, 'Default', 'Default', '', '') -- Default is PerForce
