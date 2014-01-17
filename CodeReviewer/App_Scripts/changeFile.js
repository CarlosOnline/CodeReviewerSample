/// <reference path="references.ts" />
var ChangeFile = (function () {
    function ChangeFile(data, viewModel) {
        var _this = this;
        this.comments = [];
        this.data = null;
        this.loaded = false;
        this.id = "";
        this.revisions = [];
        this.viewModel = null;
        this.selected = ko.es5.mapping.computed(function () {
            if (_this.viewModel.changeList.file == null)
                return false;

            return _this.viewModel.changeList.file.data.serverFileName == _this.data.serverFileName;
        });
        this.commentStats = ko.es5.mapping.computed(function () {
            var stats = {
                active: 0,
                resolved: 0,
                wontFix: 0,
                closed: 0,
                canceled: 0
            };
            var selectedComments = _this.viewModel.comments;

            var comments = _this.selected ? _this.viewModel.comments : _this.data.comments;
            (comments).forEach(function (comment) {
                var status = _this.selected ? (comment).status.select.idx : (comment).status;
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
            }, _this);
            return stats;
        });
        this.resource = ko.es5.mapping.computed(function () {
            var stats = _this.commentStats;
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
        this.click = function () {
            if (_this.selected)
                return;

            _this.viewModel.onFileChangeSelect(_this.data.serverFileName);
            UI.Effects.flash("#" + _this.id);
        };
        this.fileVersion = function () {
            return _this.data.fileVersions[_this.data.fileVersions.length - 1];
        };
        this.versionResource = function () {
            var fileVersion = _this.fileVersion();
            return Resource.FileVersion.getResource(fileVersion.action);
        };
        this.file = {
            value: ko.es5.mapping.computed(function () {
                return _this.data.name;
            }),
            title: ko.es5.mapping.computed(function () {
                var resource = _this.versionResource();
                return _this.data.serverFileName + " (" + resource.value + ")";
            }),
            classes: ko.es5.mapping.computed(function () {
                var selected = _this.selected ? "selected " : "";
                var fileVersion = _this.fileVersion();
                var strikeThrough = fileVersion.action == Resources.FileVersion.Delete.id ? " strikeThrough " : "";
                return selected + _this.resource.file.classes + strikeThrough;
            }),
            click: function () {
                _this.click();
            }
        };
        this.icon = {
            previous: Resources.ChangeFile.None.icon.classes,
            classes: ko.es5.mapping.computed(function () {
                var current = _this.resource.icon.classes;
                if (_this.icon.previous != current) {
                    // TODO: Restore after not recreating ChangeFile
                    //UI.Effects.flash("#" + this.id);
                    _this.icon.previous = current;
                }
                return current;
            }),
            title: ko.es5.mapping.computed(function () {
                return _this.resource.icon.title;
            }),
            click: function () {
                _this.click();
            }
        };
        this.loadRevisions = function () {
            var requests = [];
            _this.revisions.forEach(function (revision) {
                var diffHtml = revision.data.diffHtml;
                if (diffHtml != null && diffHtml != "" && diffHtml.length > 1)
                    return;

                var request = AJAX.getDiffRevision(_this.viewModel.changeList.data.id, _this.data.id, revision.tabIndex, function (data) {
                    if (data !== undefined)
                        revision.data.diffHtml = data;
                });
                requests.push(request);
            });
            return requests;
        };
        this.revisionFromFileVersion = function (fileVersionId) {
            var found = _this.revisions.first(function (revision) {
                return (revision.data.id == fileVersionId);
            });
            return found;
        };
        this.status = {
            idx: ko.es5.mapping.computed(function () {
                return _this.resource.id;
            }),
            value: ko.es5.mapping.computed(function () {
                return _this.resource.value;
            })
        };
        this.data = data;
        this.viewModel = viewModel;
        this.id = "ChangeFile" + ChangeFile._id++;

        var tabIndex = 0;
        this.data.fileVersions.forEach(function (fileVersion) {
            if (fileVersion.isRevisionBase)
                return;

            var revision = new Revision(fileVersion, tabIndex);
            _this.revisions.push(revision);
            tabIndex++;
        });

        ko.es5.mapping.track(this);
    }
    ChangeFile._id = 0;
    return ChangeFile;
})();
