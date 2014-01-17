/*
    TypeScript classes representing CodeReviewer\Model\DtoMapper.cs classes
    Keep the two files in sync.
*/

module Dto {
    export class ChangeListDto {
        type: string = "";
        changeFiles: Array<ChangeFileDto> = [];
        reviewers: Array<ReviewerDto> = [];
        reviews: Array<ReviewDto> = [];
        comments: Array<CommentGroupDto> = [];

        CL: string = "";
        //description: number = 0;
        id: number = 0;
        sourceControlId: number = 0;
        stage: number = 0;
        timeStamp: string = "";
        userClient: string = "";
        userName: string = "";
        reviewerAlias: string = "";
    }

    export class ChangeFileDto {
        type: string = "";
        changeList: ChangeListDto;
        fileVersions: Array<FileVersionDto> = [];
        comments: Array<CommentGroupDto> = [];
        changeListId: number = 0;
        id: number = 0;
        isActive: boolean = false;
        localFileName: string = "";
        serverFileName: string = "";
        name: string = "";
    }

    export class CommentGroupDto {
        type: string = "";
        id: number = 0;
        reviewId: number = 0;
        changeListId: number = 0;
        fileVersionId: number = 0;
        line: number = 0;
        lineStamp: string = "";
        status: number = 0;
        threads: Array<CommentDto> = [];
    }

    export class CommentDto {
        type: string = "";
        id: number = 0;
        commentText: string = "";
        reviewRevision: number = 0;
        reviewerId: number = 0;
        fileVersionId: number = 0;
        groupId: number = 0;
        userName: string = "";
        reviewerAlias: string = "";
    }

    export class FileVersionDto {
        changeFile: ChangeFileDto = null;
        type: string = "";
        action: number = 0;
        fileId: number = 0;
        id: number = 0;
        isFullText: boolean = false;
        isRevisionBase: boolean = false;
        isText: boolean = false;
        revision: number = 0;
        reviewRevision: number = 0;
        timeStamp: string = "";
        name: string = "";
        diffHtml: string = "";
    }

    export class MailChangeListDto {
        type: string = "";
        changeList: ChangeListDto = null;
        reviewer: ReviewerDto = null;
        ChangeListId: number = 0;
        Id: number = 0;
        ReviewerId: number = 0;
    }

    export class MailReviewDto {
        type: string = "";
        review: ReviewDto = null;
        Id: number = 0;
        ReviewId: number = 0;
    }

    export class MailReviewRequestDto {
        type: string = "";
        changeList: ChangeListDto = null;
        Id: number = 0;
        ChangeListId: number = 0;
        ReviewerAlias: string = "";
    }

    export class ReviewDto {
        type: string = "";
        changeList: ChangeListDto;
        //mailReviews: Array<MailReviewDto> = [];
        changeListId: number = 0;
        commentText: string = "";
        id: number = 0;
        isSubmitted: boolean = false;
        overallStatus: number = 0;
        timeStamp: string = "";
        userName: string = "";
        reviewerAlias: string = "";
    }

    export class ReviewerDto {
        type: string = "";
        //MailChangeLists: Array<MailChangeListDto> = [];
        changeList: ChangeListDto = null;
        changeListId: number = 0;
        id: number = 0;
        reviewerAlias: string = "";
        status: number = 0;
        // extra members
        requestType: number = 0;
    }

    export class ChangeListStatusDto {
        type: string = "";
        id: number = 0;
        status: number = 0;
    }

    export class UserContextDto {
        id: number = 0;
        userName: string = "";
        reviewerAlias: string = "";
        key: string = "";
        value: string = "";
        version: number = 0;
    }

    export module UserSettings {
        export class Dynamic {
            comments: boolean = false;
            reviewers: boolean = false;
            changeLists: boolean = false;
        }

        export class Diff {
            ignoreWhiteSpace: boolean = false;
            preDiff: boolean = false;
            intraLineDiff: boolean = false;
            DefaultSettings: boolean = false;
        }

        export class LeftPane {
            closed: boolean = false;
            width: number = 0;
        }
    }

    export class UserSettingsDto {
        key: string = "";
        broadCastDelay: number = 0;
        diff: UserSettings.Diff;
        dynamic: UserSettings.Dynamic = null;
        leftPane: UserSettings.LeftPane = null;
        dualPane: boolean = true;
        hideUnchanged: boolean = false;

        CurrentVersion: number = 0;
    }

    export module ChangeListSettings {
        export class FileSettings {
            fileId: number = -1;
            tabIndex: number = -1;
        }
    }

    export class ChangeListSettingsDto {
        key: string = "";
        defaultDiff: boolean = false;
        fontName: string = "";
        fontSize: number = 0;
        fileId: number = 0;
        fileRevision: number = 0;
        files: Array<ChangeListSettings.FileSettings> = [];
        CurrentVersion: number = 0;
    }

    export class DeleteNotification {
        type: string = "";
        id: number = 0;
        delete: string = "";
    }

    export class ChangeListDisplayItemDto {
        CL: string = "";
        description: string = "";
        stage: string = "";
        server: string = "";
        shortDescription: string = "";
        id: number = 0;
    }
}