/// <reference path="references.ts" />

class CommentThread {
    original: Dto.CommentDto;
    editMode = false;
    show = true;
    subscriptions = new SubscriptionList();

    canEdit = ko.es5.mapping.computed<boolean>(() => {
        return this.isMe() ? true : false;
    });

    display = {
        editor: ko.es5.mapping.computed<string>(() => {
            return this.editMode == false ? "none" : "inline";
        }),
        icon: {
            remove: ko.es5.mapping.computed<string>(() => {
                if (this.editMode)
                    return "none";

                if (this.data.id == 0)
                    return (this.isMe()) ? "inline" : "none";
            }),
            hide: ko.es5.mapping.computed<string>(() => {
                if (this.editMode)
                    return "none";
                return "inline";
                return "none";
                return (this.isMe()) ? "inline" : "none";
            }),
            edit: ko.es5.mapping.computed<string>(() => {
                if (this.editMode)
                    return "none";
            }),
        },
        thread: ko.es5.mapping.computed<string>(() => {
            return this.show && this.comment.line.info.fileVersionId >= this.data.fileVersionId ? "inline" : "none";
        }),
        userName: ko.es5.mapping.computed<string>(() => {
            return "inline";
        }),
        viewer: ko.es5.mapping.computed<string>(() => {
            return this.editMode ? "none" : "inline";
        }),
    };
    isMe = () => {
        return g_UserName == this.data.userName;
    };

    close = () => {
        if (this.editMode)
            return false;

        this.show = false;
        return false;
    };

    edit = () => {
        if (this.isMe())
            this.editMode = true;
        return false;
    };

    remove = () => {
        this.comment.threads.remove(this);
        if (this.data.id > 0) {
            this.comment.threadService.deleted(this);
        }
        return false;
    };

    submit = {
        click: () => {
            this.editMode = false;
            this.comment.threadService.changed(this);
            return false;
        },

        cancel: () => {
            this.editMode = false;
            if (this.data.id == 0)
                this.comment.threads.remove(this);
            else
                // Restore pre-change text
                this.data.commentText = this.original.commentText;
            return false;
        },

        enable: ko.es5.mapping.computed<boolean>(() => {
            // TODO: figure out why addNewThread version doesn't update this value
            return this.data.id == 0 || this.data.commentText.length > 0 ? true : false;
        })
    };

    updateData = (newData: Dto.CommentDto) => {
        this.data.id = newData.id;
        this.data.commentText = newData.commentText;
        this.data.userName = newData.userName;
        this.data.reviewerAlias = newData.reviewerAlias;
        this.data.groupId = newData.groupId;
    };

    constructor(public data: Dto.CommentDto, public comment: ReviewComment) {
        if (comment === undefined || comment == null) {
            throw "CommentThread: Invalid comment arg";
        }

        this.original = $.extend(true, {}, data);
        this.editMode = this.data.id == 0 ? true : false;

        ko.es5.mapping.track(this);
    }
}
