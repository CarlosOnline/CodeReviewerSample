/// <reference path="references.ts" />

module AJAX {
    export function query(postMethod: Function, methodName: string, url: string, input: any, passCallback?: Function, failCallBack?: Function, relative = false) {

        if (!relative) {
            url = g_BaseUrl + url;
        }
        console.log(methodName, url);

        var self = this;
        self.url = url;
        passCallback = passCallback || null;
        failCallBack = failCallBack || UI.defaultErrorMessage;

        try {
            return postMethod(url, input, function (data, status, xhr) {
                var error = data.Error || false;
                //console.log(methodName, url, !error, status, xhr);

                if (status == "success" && !error) {
                    if (passCallback != null) {
                        passCallback(data);
                    }
                } else {
                    if (failCallBack != null) {
                        var errorTitle = "Url Error " + methodName + " - ";
                        var response = xhr.responseText;
                        if (data.Message !== undefined && data.Message !== "")
                            response = data.Message;

                        console.log(errorTitle, self.url, status, xhr, response);
                        failCallBack(errorTitle + self.url, response);
                    }
                }
            }).fail(function (data) {
                    if (failCallBack != null) {
                        var errorTitle = "Url Error " + methodName + ".fail - ";
                        console.log(errorTitle, self.url, data.responseText, data);
                        failCallBack(errorTitle + self.url, data.responseText);
                    }
                });
        } catch (err) {
            if (failCallBack != null) {
                var errorTitle = "Url Error " + methodName + ".exception - ";
                console.log(errorTitle, self.url, err);
                failCallBack(errorTitle + self.url, err);
            }
            return null;
        }
    }

    export function getJSON(url: string, input: any, passCallback?: Function, failCallBack?: Function) {
        return query($.getJSON, "getJSON", url, input, passCallback, failCallBack);
    }

    export function getUrl(url, input, passCallback?: Function, failCallBack?: Function) {
        return query($.get, "get", url, input, passCallback, failCallBack);
    }

    export function addComment(comment: ReviewComment, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ChangeList.CL,
            id: comment.data.id,
            fileVersionId: comment.data.fileVersionId,
            line: comment.data.line, // TODO: parse line
            lineStamp: comment.data.lineStamp,
            status: comment.data.status
        };
        getJSON("Comment/Add", input, passCallback, failCallBack);
    }

    export function deleteComment(id, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ChangeList.CL,
            id: id
        };
        getJSON("Comment/Delete", input, passCallback, failCallBack);
    }

    export function addThread(thread: CommentThread, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ChangeList.CL,
            id: thread.data.id,
            reviewRevision: thread.data.reviewRevision,
            reviewerId: thread.data.reviewerId,
            fileVersionId: thread.data.fileVersionId,
            commentText: thread.data.commentText,
            userName: thread.data.userName,
            reviewerAlias: thread.data.reviewerAlias,
            groupId: thread.data.groupId,
            changeListId: thread.comment.data.changeListId,
            lineStamp: thread.comment.data.lineStamp,
            status: thread.comment.data.status
        };
        getJSON("Thread/Add", input, passCallback, failCallBack);
    }

    export function deleteThread(thread: CommentThread, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ChangeList.CL,
            id: thread.data.id
        };
        getJSON("Thread/Delete", input, passCallback, failCallBack);
    }

    export function getChangeFile(id, fileId, passCallback?: Function, failCallBack?: Function) {
        var input = {
            id: id,
            fileId: fileId
        };
        getJSON("Diff/File", input, passCallback, failCallBack);
    }

    export function getDiffRevision(id, fileId, revisionId, passCallback?: Function, failCallBack?: Function) {
        var input = {
            id: id,
            fileId: fileId,
            revisionId: revisionId
        };
        return getUrl("Diff/Revision", input, passCallback, failCallBack);
    }

    export function addReviewer(reviewer: Dto.ReviewerDto, passCallback?: Function, failCallBack?: Function) {
        if ((<any>reviewer).__ko_mapping__ != undefined)
            reviewer = ko.mapping.toJS(reviewer);

        var input = {
            CL: g_ChangeList.CL,
            id: reviewer.id,
            //userName: reviewer.userName,
            reviewerAlias: reviewer.reviewerAlias,
            changeListId: reviewer.changeListId,
            status: reviewer.status,
            requestType: reviewer.requestType != undefined ? reviewer.requestType : -1,
        };
        getJSON("Reviewer/Add", input, passCallback, failCallBack);
    }

    export function deleteReviewer(id, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ChangeList.CL,
            id: id
        };
        getJSON("Reviewer/Delete", input, passCallback, failCallBack);
    }

    export function setChangeListStatus(status, passCallback?: Function, failCallBack?: Function) {
        var input = {
            CL: g_ViewModel.changeList.data.CL,
            "id": g_ViewModel.changeList.data.id,
            "status": status
        };
        getJSON("Diff/Status", input, passCallback, failCallBack);
    }

    export function pingChangeListReviewers(id, passCallback?: Function, failCallBack?: Function) {
        var input = {
            "id": id
        };
        getJSON("Diff/Ping", input, passCallback, failCallBack);
    }

    export function getSettings(id, key, passCallback?: Function, failCallBack?: Function) {
        var input = {
            "id": id,
            "key": key,
            userName: g_UserName,
        };
        getJSON("Settings", input, passCallback, failCallBack);
    }

    export function updateSettings(id, key, data, passCallback?: Function, failCallBack?: Function) {
        if (data.__ko_mapping__ != undefined)
            data = ko.mapping.toJS(data);
        data["key"] = key;

        var input = {
            "id": id,
            "key": key,
            userName: g_UserName,
            value: JSON.stringify(data)
        };
        getJSON("Settings/Update", input, passCallback, failCallBack);
    }
}
