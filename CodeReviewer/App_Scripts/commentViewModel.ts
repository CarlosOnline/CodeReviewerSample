/// <reference path="references.ts" />

module ViewModel {
    export class Comment {
        view: Diff;

        init = (view) => {
            this.view = view;
        };

        addComment = (td) => {
            var lineId = td.id;
            var lineStampInfo = getLineStampInfo(td.id);
            if (lineStampInfo.type == "Base")
                return;

            var thread = new Dto.CommentDto();
            thread.userName = g_UserName;
            thread.reviewerAlias = g_ReviewerAlias;
            thread.fileVersionId = lineStampInfo.fileVersionId;
            ko.es5.mapping.track(thread);

            var data = new Dto.CommentGroupDto();
            data.changeListId = this.view.changeList.data.id;
            data.fileVersionId = lineStampInfo.fileVersionId;
            data.lineStamp = lineId;
            data.threads.push(thread);
            ko.es5.mapping.track(data);

            var comment = new ReviewComment(data, this.view, true);
            if (comment.valid) {
                this.view.comments.push(comment);
                this.sort();
            }
        };

        findCommentByLine = (td: HTMLElement) => {
            return this.view.comments.first((comment) => {
                return (comment.line.td.id == td.id);
            });
        }

        getUniqueLineStamp = (lineStamp: string) => {
            var parts = lineStamp.split("_");
            var baseLineStamp = parts[0];
            var idx = parts.length == 2 ? parseInt(parts[1]) : 0;
            var unique = lineStamp;
            do {
                var found = this.view.changeList.file.data.comments.filter((comment) => {
                    return comment.lineStamp == unique;
                });

                if (found == null || found.length <= 1)
                    return unique;

                unique = baseLineStamp + "_" + idx;
                idx++;
            } while (true);
        };

        getTD = (lineStamp: string) => {
            var td = findLineInRevision(lineStamp, this.view.tabIndex);
            if (td == null)
                return null;

            // check if td is already in use by a previous comment
            var found = this.findCommentByLine(td);
            if (found != null) {
                var tr = <HTMLTableRowElement> td.parentElement;
                lineStamp = this.getUniqueLineStamp(lineStamp);
                td = document.getElementById(lineStamp);
                if (td == null) {
                    var template = document.getElementById("uniqueLineTemplate");
                    var holder = document.getElementById("templateHolder");
                    var rowId = "Unique-Row-" + lineStamp;
                    $(template.innerHTML).clone().attr("id", rowId).insertAfter(holder);
                    var row = <HTMLTableRowElement> document.getElementById(rowId);
                    td = <HTMLTableCellElement>row.cells[1];
                    td.id = lineStamp;
                }
            }
            return td;
        };

        sort = () => {
            this.view.comments.sort((left, right) => {
                if (left.line.tr.rowIndex == right.line.tr.rowIndex)
                    return 0;

                return left.line.tr.rowIndex < right.line.tr.rowIndex ? -1 : 1;
            });
        };

        updateComment = (data: Dto.CommentGroupDto) => {
            var comment = this.view.comments.first((item) => {
                return item.data.lineStamp == data.lineStamp;
            });

            ko.es5.mapping.track(data);

            if (comment != null) {
                comment.updateData(data);
            } else {
                comment = new ReviewComment(data, this.view);
                if (comment.valid)
                    this.view.comments.push(comment);
            }
        };

        removeComment = (id) => {
            var comment = this.view.comments.first((item) => {
                return item.data.id == id;
            });

            if (comment == null) {
                console.log("did not find comment", id);
                return;
            }

            comment.dispose();
            comment.remove();
            delete comment;
        };

        syncComments = () => {
            if (this.view.changeList.file == null)
                return;

            var list: Array<Dto.CommentGroupDto> = [];
            this.view.comments.forEach((comment) => {
                list.push(comment.data);
            });

            this.view.changeList.file.data.comments = list;
        };
    } // comment


    export function getLineStampInfo(id: string) {
        // Edge-2-2-3-3-7-8_0
        // Diff-1-139-140-140-16_11
        //      1: reviewRevision, 
        //      2: leftId, 
        //      3: rightId, 
        //      4: fileId, 
        //      5: leftLine, 
        //      6: rightline
        var data = id.split('-');
        var info = new UI.LineStamp.Info();
        if (data.length < 7)
            console.log("getLineStampInfo", id);

        info.reviewRevision = parseInt(data[1]);
        info.id = id;
        info.fileVersionId = parseInt(data[4]);

        info.left.fileVersionId = parseInt(data[2]);
        info.right.fileVersionId = parseInt(data[3]);

        info.left.lineNumber = data[5];
        var num = info.left.lineNumber.split('_');
        info.left.lineBase = num.length > 0 ? num[0] : "";
        info.left.lineOffset = num.length > 1 ? num[1] : "";

        info.right.lineNumber = data[6];
        num = info.right.lineNumber.split('_');
        info.right.lineBase = num.length > 0 ? num[0] : "";
        info.right.lineOffset = num.length > 1 ? num[1] : "";

        info.left.other = info.right;
        info.right.other = info.left;

        info.type = info.fileVersionId == info.left.fileVersionId ? "Base" : "Diff";
        info.side = info.type == "Base" ? info.left : info.right;
        info.side.id = info.id;
        info.side.other.id = encodeOtherLineStamp(info);
        return <any> info;
    }

    export function encodeOtherLineStamp(info) {
        // encode next lineStamp:
        // Base-2-2-3-2-2-2
        var left = info.fileVersionId == info.left.fileVersionId ? true : false;
        var parts = [];
        parts.push(left ? "Diff" : "Base");
        parts.push(info.reviewRevision);
        parts.push(info.left.fileVersionId);
        parts.push(info.right.fileVersionId);
        parts.push(left ? info.right.fileVersionId : info.left.fileVersionId);
        parts.push(info.left.lineBase);
        parts.push(info.right.lineBase);
        var id = parts.join('-');
        return id;
    }

    export function encodeNextRevisionLineStamp(info, nextReviewRevision, nextFileVersionId) {
        // encode next lineStamp:
        // Base-2-2-3-2-2
        var parts = [];
        parts.push(nextReviewRevision);
        parts.push(info.fileVersionId);
        parts.push(nextFileVersionId);
        parts.push(info.fileVersionId);
        parts.push(info.side.lineBase);
        var id = 'Base-' + parts.join('-');
        return id;
    }

    export function findLineInRevision(lineStamp, targetRevision) {
        var info = getLineStampInfo(lineStamp);
        if (info.fileVersionId == g_ViewModel.revision.data.id) {
            return document.getElementById(lineStamp);
        }

        var revision = g_ViewModel.changeList.file.revisionFromFileVersion(info.fileVersionId);
        if (revision == null || info.fileVersionId >= g_ViewModel.revision.data.id) {
            return null;
        }

        if (info.fileVersionId < g_ViewModel.revision.data.id) {
            for (var idx = revision.tabIndex + 1; idx <= targetRevision; idx++) {
                var curRevision = g_ViewModel.changeList.file.revisions[idx];
                var id = encodeNextRevisionLineStamp(info, curRevision.data.reviewRevision, curRevision.data.id);
                var found = $("[id ^= " + id + "]");
                if (found == null || found.length == 0)
                    return null;

                var foundElem = found[0];
                info = getLineStampInfo(foundElem.id);
                var diffId = info.side.other.id;
                var found = $("[id ^= " + diffId + "]");
                if (found == null || found.length == 0)
                    return null;

                foundElem = found[0];
                info = getLineStampInfo(foundElem.id);
            }
        }
        var td = document.getElementById(info.id);
        return td; // handle pseudo elements
    }
}
