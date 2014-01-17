/// <reference path="references.ts" />

class Navigation {
    baseChanges: JQuery = $();
    _comments: Array<ReviewComment> = [];
    commentsChanged = 0;
    cookies: Cookies = null;
    currentFile: ChangeFile = null;
    currentComment: ReviewComment = null;
    diffChanges: JQuery = $();
    idxChange = -1;
    viewModel: ViewModel.Diff = null;

    comments: Array<ReviewComment> = ko.es5.mapping.computed(() => {
        if (this.commentsChanged >= 0) {
            // check to include observable in computed
            //console.log(this.commentsChanged);
        }

        this._comments.removeAll();
        g_ViewModel.comments.forEach((comment) => {
            if (comment.line.td != null && $(comment.line.td).is(":visible") &&
                comment.qtipElem != null && $(comment.qtipElem).is(":visible"))
                this._comments.push(comment);
        });
        return this._comments;
    });

    cookieName = () => {
        return "navigation_" + this.viewModel.changeList.file.data.id + "_" + this.viewModel.revision.tabIndex;
    };

    idxComment = ko.es5.mapping.computed(() => {
        if (this.comments.length > 0 && this.currentComment != null) {
            return this.comments.indexOf(this.currentComment);
        }
        return -1;
    });

    hideUnchangedLines = {
        classes: ko.es5.mapping.computed(() => {
            return this.viewModel.settings.settings.user.hideUnchanged ? "ui-icon ui-icon-circle-plus" : "ui-icon ui-icon-circle-minus";
        }),

        click: () => {
            this.viewModel.settings.settings.user.hideUnchanged = !this.viewModel.settings.settings.user.hideUnchanged;
            this.viewModel.settings.apply();
            this.resetComments();
            setTimeout(() => {
                this.commentsChanged++;
            }, 100);
        },

        display: () => { return ""; },

        title: ko.es5.mapping.computed(() => {
            return this.viewModel.settings.settings.user.hideUnchanged ? "Show Unchanged Lines" : "Hide Unchanged Lines";
        }),
    };

    previousChange = {
        classes: ko.es5.mapping.computed(() => { return this.diffChanges.length > 0 && this.idxChange > 0 ? "" : "ui-state-disabled"; }),

        click: () => {
            this.idxChange--;
            this.showCurrentChange();
        },

        display: () => { return ""; },
    };

    nextChange = {
        classes: ko.es5.mapping.computed(() => { return this.diffChanges.length > 0 && this.idxChange < this.diffChanges.length - 1 ? "" : "ui-state-disabled"; }),

        click: () => {
            this.idxChange++;
            this.showCurrentChange();
        },

        display: () => { return ""; },
    };

    previousComment = {
        classes: ko.es5.mapping.computed(() => { return this.comments.length > 0 && this.idxComment < this.comments.length - 1 ? "" : "ui-state-disabled"; }),
        click: () => {
            this.showCurrentComment(this.idxComment - 1);
        },
        display: () => { return ""; },
    };

    nextComment = {
        classes: ko.es5.mapping.computed(() => {
            return this.comments.length > 0 && this.idxComment < this.comments.length - 1 ? "" : "ui-state-disabled";
        }),
        click: () => {
            this.showCurrentComment( + 1);
        },
        display: () => { return ""; },
    };

    previousFile = {
        classes: ko.es5.mapping.computed(() => {
            return this.idxFile >= 1 ? "" : "ui-state-disabled";
        }),
        click: () => {
            this.showCurrentFile(this.idxFile - 1);
        },
        display: () => { return ""; },
    };

    nextFile = {
        classes: ko.es5.mapping.computed(() => {
            return this.idxFile >= 0 && this.idxFile < this.viewModel.changeList.changeFiles.length - 1 ? "" : "ui-state-disabled";
        }),
        click: () => {
            this.showCurrentFile(this.idxFile + 1);
        },
        display: () => { return ""; },
    };

    idxFile = ko.es5.mapping.property(ko.es5.mapping.computed(() => {
        if (this.viewModel.changeList.file == null)
            return -1;
        return this.viewModel.changeList.changeFiles.indexOf(this.viewModel.changeList.file);
    }));

    loadChanges = () => {
        var baseTable = $(UI.Revisions.Current.LeftTable).filter(":visible");
        var exclude = baseTable.find('tbody.Unchanged,tbody.Seperator');
        this.baseChanges = baseTable.find('tbody').not(exclude).filter(":visible");
        this.baseChanges.removeClass("CurrentChange");

        var diffTable = $(UI.Revisions.Current.RightTable).filter(":visible");
        exclude = diffTable.find('tbody.Unchanged,tbody.Seperator');
        this.diffChanges = diffTable.find('tbody').not(exclude).filter(":visible");
        this.diffChanges.removeClass("CurrentChange");
    };

    load = () => {
        this.loadChanges();

        this.idxChange = -1;
        this.currentComment = null;

        this.cookies.load();
        this.showCurrentChange();

        if (this.idxChange == 0)
            this.idxChange--; // reset to allow first F8 to go to first change
    };

    reset = () => {
        this.resetComments();
        this.resetFiles();
        this.baseChanges = $();
        this.diffChanges = $();
        this.idxChange = -1;
    };

    resetComments = () => {
        if (this.currentComment != null && this.currentComment.qtipElem != null) {
            var qtip = $(this.currentComment.qtipElem);
            qtip.removeClass("qtip-CurrentChange");
            qtip.addClass(this.currentComment.getResource().qtip.classes);
        }

        this.currentComment = null;
    };

    resetFiles = () => {
        this.currentFile = null;
    };

    private showCurrentChange = () => {
        this.loadChanges();
        if (this.diffChanges.length === 0)
            return;

        if (this.idxChange < 0)
            this.idxChange = 0;
        else if (this.idxChange >= this.diffChanges.length)
            this.idxChange = this.diffChanges.length - 1;

        var elem = this.baseChanges[this.idxChange];
        $(elem).addClass("CurrentChange");
        scrollToMiddle(UI.Revisions.Current.LeftTable, elem);

        elem = this.diffChanges[this.idxChange];
        $(elem).addClass("CurrentChange");
        scrollToMiddle(UI.Revisions.Current.RightTable, elem);
    };

    private showCurrentComment = (idx: number) => {
        if (this.comments.length === 0)
            idx = -1;
        else if (idx <= 0)
            idx = 0;
        else if (idx + 1 >= this.comments.length)
            idx = this.comments.length - 1;

        var nextComment = idx >= 0 ? this.comments[idx] : null;
        if (nextComment == this.currentComment)
            return;

        this.resetComments();
        this.currentComment = nextComment;
        if (this.currentComment == null || this.currentComment.qtipElem == null)
            throw "Could not find comment for qtip";

        var qtip = $(this.currentComment.qtipElem);
        qtip.removeClass(this.currentComment.getResource().qtip.classes);
        qtip.addClass("qtip-CurrentChange");

        scrollToMiddle(UI.Revisions.Current.LeftTable, this.currentComment.placeHolders[0].ux);
        scrollToMiddle(UI.Revisions.Current.RightTable, this.currentComment.placeHolders[1].ux);
    };

    private showCurrentFile = (idx: number) => {
        if (this.viewModel.changeList.changeFiles.length === 0)
            idx = -1;
        else if (idx <= 0)
            idx = 0;
        else if (idx + 1 >= this.viewModel.changeList.changeFiles.length)
            idx = this.viewModel.changeList.changeFiles.length - 1;

        var next = idx >= 0 ? this.viewModel.changeList.changeFiles[idx] : null;
        if (next == null || next == this.currentFile)
            return;

        this.resetFiles();
        this.currentFile = next;
        if (this.currentFile == null)
            throw "Could not find current file";

        this.viewModel.onFileChangeSelect(next.data.serverFileName);
    };

    showUnchangedLinesBody = (tbody: HTMLElement, callback: () => any = null) => {
        callback = callback || function () { };
        UI.Effects.hide(tbody, () => {
            var tbodyHidden = <HTMLElement> tbody.previousElementSibling;
            UI.Effects.show(tbodyHidden, callback);
        });
    };

    showUnchangedLines = (button: HTMLElement) => {
        var td = getTD(button);
        var tbody = td.parentElement.parentElement;
        this.showUnchangedLinesBody(tbody);

        var trSrc = <HTMLTableRowElement> td.parentElement;
        var idxSrc = trSrc.rowIndex;

        var tableTgt = UI.Revisions.getPartnerTable(trSrc);
        if (idxSrc >= tableTgt.rows.length)
            return;

        var trTgt = <HTMLTableRowElement> tableTgt.rows[idxSrc];
        if (trTgt == null)
            return;

        var tbody = trTgt.parentElement;
        this.showUnchangedLinesBody(tbody, () => {
            var hidden = $(UI.Revisions.Current.DiffTable).find("tbody.Unchanged").filter(':hidden').length;
            if (hidden == 0)
                this.viewModel.settings.settings.user.hideUnchanged = false;
            this.viewModel.updatePosition();
        });
    }

    constructor(viewModel: ViewModel.Diff) {
        this.viewModel = viewModel;
        this.reset();
        ko.es5.mapping.track(this);

        this.viewModel.events.changed.add((type: number) => {
            if (type != ViewModel.DiffEvents.Revision)
                return;

            this.reset();
            this.cookies.setName(this.cookieName());
            this.load();
            this.cookies.subscribe();
            setTimeout(() => {
                this.commentsChanged++;
            }, 100);
            $("input.Seperator").click((eventObj: JQueryEventObject) => {
                this.showUnchangedLines(<HTMLElement> eventObj.target);
            });
        });

        this.viewModel.events.unloaded.add((type: number) => {
            this.cookies.dispose();
            this.reset();
        });

        this.cookies = new Cookies(this.cookieName(), [
            ko.getObservable(this, "idxChange"),
        ]);
    }
}
