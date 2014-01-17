/*
TypeScript classes representing CodeReviewer\Model\DtoMapper.cs classes
Keep the two files in sync.
*/
var Dto;
(function (Dto) {
    var ChangeListDto = (function () {
        function ChangeListDto() {
            this.type = "";
            this.changeFiles = [];
            this.reviewers = [];
            this.reviews = [];
            this.comments = [];
            this.CL = "";
            //description: number = 0;
            this.id = 0;
            this.sourceControlId = 0;
            this.stage = 0;
            this.timeStamp = "";
            this.userClient = "";
            this.userName = "";
            this.reviewerAlias = "";
        }
        return ChangeListDto;
    })();
    Dto.ChangeListDto = ChangeListDto;

    var ChangeFileDto = (function () {
        function ChangeFileDto() {
            this.type = "";
            this.fileVersions = [];
            this.comments = [];
            this.changeListId = 0;
            this.id = 0;
            this.isActive = false;
            this.localFileName = "";
            this.serverFileName = "";
            this.name = "";
        }
        return ChangeFileDto;
    })();
    Dto.ChangeFileDto = ChangeFileDto;

    var CommentGroupDto = (function () {
        function CommentGroupDto() {
            this.type = "";
            this.id = 0;
            this.reviewId = 0;
            this.changeListId = 0;
            this.fileVersionId = 0;
            this.line = 0;
            this.lineStamp = "";
            this.status = 0;
            this.threads = [];
        }
        return CommentGroupDto;
    })();
    Dto.CommentGroupDto = CommentGroupDto;

    var CommentDto = (function () {
        function CommentDto() {
            this.type = "";
            this.id = 0;
            this.commentText = "";
            this.reviewRevision = 0;
            this.reviewerId = 0;
            this.fileVersionId = 0;
            this.groupId = 0;
            this.userName = "";
            this.reviewerAlias = "";
        }
        return CommentDto;
    })();
    Dto.CommentDto = CommentDto;

    var FileVersionDto = (function () {
        function FileVersionDto() {
            this.changeFile = null;
            this.type = "";
            this.action = 0;
            this.fileId = 0;
            this.id = 0;
            this.isFullText = false;
            this.isRevisionBase = false;
            this.isText = false;
            this.revision = 0;
            this.reviewRevision = 0;
            this.timeStamp = "";
            this.name = "";
            this.diffHtml = "";
        }
        return FileVersionDto;
    })();
    Dto.FileVersionDto = FileVersionDto;

    var MailChangeListDto = (function () {
        function MailChangeListDto() {
            this.type = "";
            this.changeList = null;
            this.reviewer = null;
            this.ChangeListId = 0;
            this.Id = 0;
            this.ReviewerId = 0;
        }
        return MailChangeListDto;
    })();
    Dto.MailChangeListDto = MailChangeListDto;

    var MailReviewDto = (function () {
        function MailReviewDto() {
            this.type = "";
            this.review = null;
            this.Id = 0;
            this.ReviewId = 0;
        }
        return MailReviewDto;
    })();
    Dto.MailReviewDto = MailReviewDto;

    var MailReviewRequestDto = (function () {
        function MailReviewRequestDto() {
            this.type = "";
            this.changeList = null;
            this.Id = 0;
            this.ChangeListId = 0;
            this.ReviewerAlias = "";
        }
        return MailReviewRequestDto;
    })();
    Dto.MailReviewRequestDto = MailReviewRequestDto;

    var ReviewDto = (function () {
        function ReviewDto() {
            this.type = "";
            //mailReviews: Array<MailReviewDto> = [];
            this.changeListId = 0;
            this.commentText = "";
            this.id = 0;
            this.isSubmitted = false;
            this.overallStatus = 0;
            this.timeStamp = "";
            this.userName = "";
            this.reviewerAlias = "";
        }
        return ReviewDto;
    })();
    Dto.ReviewDto = ReviewDto;

    var ReviewerDto = (function () {
        function ReviewerDto() {
            this.type = "";
            //MailChangeLists: Array<MailChangeListDto> = [];
            this.changeList = null;
            this.changeListId = 0;
            this.id = 0;
            this.reviewerAlias = "";
            this.status = 0;
            // extra members
            this.requestType = 0;
        }
        return ReviewerDto;
    })();
    Dto.ReviewerDto = ReviewerDto;

    var ChangeListStatusDto = (function () {
        function ChangeListStatusDto() {
            this.type = "";
            this.id = 0;
            this.status = 0;
        }
        return ChangeListStatusDto;
    })();
    Dto.ChangeListStatusDto = ChangeListStatusDto;

    var UserContextDto = (function () {
        function UserContextDto() {
            this.id = 0;
            this.userName = "";
            this.reviewerAlias = "";
            this.key = "";
            this.value = "";
            this.version = 0;
        }
        return UserContextDto;
    })();
    Dto.UserContextDto = UserContextDto;

    (function (UserSettings) {
        var Dynamic = (function () {
            function Dynamic() {
                this.comments = false;
                this.reviewers = false;
                this.changeLists = false;
            }
            return Dynamic;
        })();
        UserSettings.Dynamic = Dynamic;

        var Diff = (function () {
            function Diff() {
                this.ignoreWhiteSpace = false;
                this.preDiff = false;
                this.intraLineDiff = false;
                this.DefaultSettings = false;
            }
            return Diff;
        })();
        UserSettings.Diff = Diff;

        var LeftPane = (function () {
            function LeftPane() {
                this.closed = false;
                this.width = 0;
            }
            return LeftPane;
        })();
        UserSettings.LeftPane = LeftPane;
    })(Dto.UserSettings || (Dto.UserSettings = {}));
    var UserSettings = Dto.UserSettings;

    var UserSettingsDto = (function () {
        function UserSettingsDto() {
            this.key = "";
            this.broadCastDelay = 0;
            this.dynamic = null;
            this.leftPane = null;
            this.dualPane = true;
            this.hideUnchanged = false;
            this.CurrentVersion = 0;
        }
        return UserSettingsDto;
    })();
    Dto.UserSettingsDto = UserSettingsDto;

    (function (ChangeListSettings) {
        var FileSettings = (function () {
            function FileSettings() {
                this.fileId = -1;
                this.tabIndex = -1;
            }
            return FileSettings;
        })();
        ChangeListSettings.FileSettings = FileSettings;
    })(Dto.ChangeListSettings || (Dto.ChangeListSettings = {}));
    var ChangeListSettings = Dto.ChangeListSettings;

    var ChangeListSettingsDto = (function () {
        function ChangeListSettingsDto() {
            this.key = "";
            this.defaultDiff = false;
            this.fontName = "";
            this.fontSize = 0;
            this.fileId = 0;
            this.fileRevision = 0;
            this.files = [];
            this.CurrentVersion = 0;
        }
        return ChangeListSettingsDto;
    })();
    Dto.ChangeListSettingsDto = ChangeListSettingsDto;

    var DeleteNotification = (function () {
        function DeleteNotification() {
            this.type = "";
            this.id = 0;
            this.delete = "";
        }
        return DeleteNotification;
    })();
    Dto.DeleteNotification = DeleteNotification;

    var ChangeListDisplayItemDto = (function () {
        function ChangeListDisplayItemDto() {
            this.CL = "";
            this.description = "";
            this.stage = "";
            this.server = "";
            this.shortDescription = "";
            this.id = 0;
        }
        return ChangeListDisplayItemDto;
    })();
    Dto.ChangeListDisplayItemDto = ChangeListDisplayItemDto;
})(Dto || (Dto = {}));
