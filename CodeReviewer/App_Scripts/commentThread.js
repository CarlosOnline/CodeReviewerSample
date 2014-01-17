/// <reference path="references.ts" />
var CommentThread = (function () {
    function CommentThread(data, comment) {
        var _this = this;
        this.data = data;
        this.comment = comment;
        this.editMode = false;
        this.show = true;
        this.subscriptions = new SubscriptionList();
        this.canEdit = ko.es5.mapping.computed(function () {
            return _this.isMe() ? true : false;
        });
        this.display = {
            editor: ko.es5.mapping.computed(function () {
                return _this.editMode == false ? "none" : "inline";
            }),
            icon: {
                remove: ko.es5.mapping.computed(function () {
                    if (_this.editMode)
                        return "none";

                    if (_this.data.id == 0)
                        return (_this.isMe()) ? "inline" : "none";
                }),
                hide: ko.es5.mapping.computed(function () {
                    if (_this.editMode)
                        return "none";
                    return "inline";
                    return "none";
                    return (_this.isMe()) ? "inline" : "none";
                }),
                edit: ko.es5.mapping.computed(function () {
                    if (_this.editMode)
                        return "none";
                })
            },
            thread: ko.es5.mapping.computed(function () {
                return _this.show && _this.comment.line.info.fileVersionId >= _this.data.fileVersionId ? "inline" : "none";
            }),
            userName: ko.es5.mapping.computed(function () {
                return "inline";
            }),
            viewer: ko.es5.mapping.computed(function () {
                return _this.editMode ? "none" : "inline";
            })
        };
        this.isMe = function () {
            return g_UserName == _this.data.userName;
        };
        this.close = function () {
            if (_this.editMode)
                return false;

            _this.show = false;
            return false;
        };
        this.edit = function () {
            if (_this.isMe())
                _this.editMode = true;
            return false;
        };
        this.remove = function () {
            _this.comment.threads.remove(_this);
            if (_this.data.id > 0) {
                _this.comment.threadService.deleted(_this);
            }
            return false;
        };
        this.submit = {
            click: function () {
                _this.editMode = false;
                _this.comment.threadService.changed(_this);
                return false;
            },
            cancel: function () {
                _this.editMode = false;
                if (_this.data.id == 0)
                    _this.comment.threads.remove(_this);
else
                    // Restore pre-change text
                    _this.data.commentText = _this.original.commentText;
                return false;
            },
            enable: ko.es5.mapping.computed(function () {
                // TODO: figure out why addNewThread version doesn't update this value
                return _this.data.id == 0 || _this.data.commentText.length > 0 ? true : false;
            })
        };
        this.updateData = function (newData) {
            _this.data.id = newData.id;
            _this.data.commentText = newData.commentText;
            _this.data.userName = newData.userName;
            _this.data.reviewerAlias = newData.reviewerAlias;
            _this.data.groupId = newData.groupId;
        };
        if (comment === undefined || comment == null) {
            throw "CommentThread: Invalid comment arg";
        }

        this.original = $.extend(true, {}, data);
        this.editMode = this.data.id == 0 ? true : false;

        ko.es5.mapping.track(this);
    }
    return CommentThread;
})();
