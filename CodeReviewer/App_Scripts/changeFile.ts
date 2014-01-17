/// <reference path="references.ts" />

class ChangeFile {
    static _id = 0;

    comments: Array<ReviewComment> = [];
    data: Dto.ChangeFileDto = null;
    loaded = false;
    id = "";
    revisions: Array<Revision> = [];
    viewModel: ViewModel.Diff = null;

    selected = ko.es5.mapping.computed<boolean>(() => {
        if (this.viewModel.changeList.file == null)
            return false;

        return this.viewModel.changeList.file.data.serverFileName == this.data.serverFileName;
    });

    commentStats = ko.es5.mapping.computed(() => {
        var stats = {
            active: 0,
            resolved: 0,
            wontFix: 0,
            closed: 0,
            canceled: 0,
        };
        var selectedComments = this.viewModel.comments; // ensure ko.computed knows about used observables

        var comments = this.selected ? <any> this.viewModel.comments : <any>this.data.comments;
        (<Array>comments).forEach((comment: any) => {
            var status = this.selected ? (<ReviewComment> comment).status.select.idx : (<Dto.CommentGroupDto> comment).status;
            switch (status) {
                case Resources.ChangeFile.Active.id:
                    stats.active++;
                    break;
                case Resources.ChangeFile.Resolved.id:
                    stats.resolved++;
                    break;
                case Resources.ChangeFile.WontFix.id:
                    stats.wontFix++;
                    break;
                case Resources.ChangeFile.Closed.id:
                    stats.closed++;
                    break;
                case Resources.ChangeFile.Canceled.id:
                    stats.canceled++;
                    break;
            }
        }, this);
        return stats;
    });

    resource = ko.es5.mapping.computed<Resource.Types.ChangeFile>(() => {
        var stats = this.commentStats;
        if (stats.active > 0)
            return Resources.ChangeFile.Active;
        else if (stats.resolved > 0)
            return Resources.ChangeFile.Resolved;
        else if (stats.wontFix > 0)
            return Resources.ChangeFile.WontFix;
        else if (stats.closed > 0)
            return Resources.ChangeFile.Closed;
        else if (stats.canceled > 0)
            return Resources.ChangeFile.Canceled;
        return Resources.ChangeFile.None;
    });

    click = () => {
        if (this.selected)
            return;

        this.viewModel.onFileChangeSelect(this.data.serverFileName);
        UI.Effects.flash("#" + this.id);
    };

    fileVersion = () => {
        return this.data.fileVersions[this.data.fileVersions.length - 1];
    };

    versionResource = () => {
        var fileVersion = this.fileVersion();
        return Resource.FileVersion.getResource(fileVersion.action);
    };

    file = {
        value: ko.es5.mapping.computed<string>(() => {
            return this.data.name;
        }),

        title: ko.es5.mapping.computed<string>(() => {
            var resource = this.versionResource();
            return this.data.serverFileName + " (" + resource.value + ")";
        }),

        classes: ko.es5.mapping.computed<string>(() => {
            var selected = this.selected ? "selected " : "";
            var fileVersion = this.fileVersion();
            var strikeThrough = fileVersion.action == Resources.FileVersion.Delete.id ? " strikeThrough " : "";
            return selected + this.resource.file.classes + strikeThrough;
        }),

        click: () => { this.click(); },
    };

    icon = {
        previous: Resources.ChangeFile.None.icon.classes,

        classes: ko.es5.mapping.computed<string>(() => {
            var current = this.resource.icon.classes;
            if (this.icon.previous != current) {
                // TODO: Restore after not recreating ChangeFile
                //UI.Effects.flash("#" + this.id);
                this.icon.previous = current;
            }
            return current;
        }),

        title: ko.es5.mapping.computed<string>(() => {
            return this.resource.icon.title;
        }),

        click: () => { this.click() },
    };

    loadRevisions = () => {
        var requests = [];
        this.revisions.forEach((revision) => {
            var diffHtml = revision.data.diffHtml
            if (diffHtml != null && diffHtml != "" && diffHtml.length > 1)
                return;

            var request = AJAX.getDiffRevision(this.viewModel.changeList.data.id, this.data.id, revision.tabIndex, (data) => {
                if (data !== undefined)
                    revision.data.diffHtml = data;
            });
            requests.push(request);
        });
        return requests;
    };

    revisionFromFileVersion = (fileVersionId) => {
        var found = this.revisions.first((revision) => {
            return (revision.data.id == fileVersionId);
        });
        return found;
    };

    status = {
        idx: ko.es5.mapping.computed<number>(() => {
            return this.resource.id;
        }),

        value: ko.es5.mapping.computed<string>(() => {
            return this.resource.value;
        }),
    };

    constructor(data: Dto.ChangeFileDto, viewModel: ViewModel.Diff) {
        this.data = data;
        this.viewModel = viewModel;
        this.id = "ChangeFile" + ChangeFile._id++; //this.data.serverFileName.replace(/[ \/.]/g, "-");

        var tabIndex = 0;
        this.data.fileVersions.forEach((fileVersion) => {
            if (fileVersion.isRevisionBase)
                return;

            var revision = new Revision(fileVersion, tabIndex);
            this.revisions.push(revision);
            tabIndex++;
        });

        ko.es5.mapping.track(this);
    }
}