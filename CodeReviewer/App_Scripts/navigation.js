/// <reference path="references.ts" />
var Navigation = (function () {
    function Navigation(viewModel) {
        var _this = this;
        this.baseChanges = $();
        this._comments = [];
        this.commentsChanged = 0;
        this.cookies = null;
        this.currentFile = null;
        this.currentComment = null;
        this.diffChanges = $();
        this.idxChange = -1;
        this.viewModel = null;
        this.comments = ko.es5.mapping.computed(function () {
            if (_this.commentsChanged >= 0) {
                // check to include observable in computed
                //console.log(this.commentsChanged);
            }

            _this._comments.removeAll();
            g_ViewModel.comments.forEach(function (comment) {
                if (comment.line.td != null && $(comment.line.td).is(":visible") && comment.qtipElem != null && $(comment.qtipElem).is(":visible"))
                    _this._comments.push(comment);
            });
            return _this._comments;
        });
        this.cookieName = function () {
            return "navigation_" + _this.viewModel.changeList.file.data.id + "_" + _this.viewModel.revision.tabIndex;
        };
        this.idxComment = ko.es5.mapping.computed(function () {
            if (_this.comments.length > 0 && _this.currentComment != null) {
                return _this.comments.indexOf(_this.currentComment);
            }
            return -1;
        });
        this.hideUnchangedLines = {
            classes: ko.es5.mapping.computed(function () {
                return _this.viewModel.settings.settings.user.hideUnchanged ? "ui-icon ui-icon-circle-plus" : "ui-icon ui-icon-circle-minus";
            }),
            click: function () {
                _this.viewModel.settings.settings.user.hideUnchanged = !_this.viewModel.settings.settings.user.hideUnchanged;
                _this.viewModel.settings.apply();
                _this.resetComments();
                setTimeout(function () {
                    _this.commentsChanged++;
                }, 100);
            },
            display: function () {
                return "";
            },
            title: ko.es5.mapping.computed(function () {
                return _this.viewModel.settings.settings.user.hideUnchanged ? "Show Unchanged Lines" : "Hide Unchanged Lines";
            })
        };
        this.previousChange = {
            classes: ko.es5.mapping.computed(function () {
                return _this.diffChanges.length > 0 && _this.idxChange > 0 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.idxChange--;
                _this.showCurrentChange();
            },
            display: function () {
                return "";
            }
        };
        this.nextChange = {
            classes: ko.es5.mapping.computed(function () {
                return _this.diffChanges.length > 0 && _this.idxChange < _this.diffChanges.length - 1 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.idxChange++;
                _this.showCurrentChange();
            },
            display: function () {
                return "";
            }
        };
        this.previousComment = {
            classes: ko.es5.mapping.computed(function () {
                return _this.comments.length > 0 && _this.idxComment < _this.comments.length - 1 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.showCurrentComment(_this.idxComment - 1);
            },
            display: function () {
                return "";
            }
        };
        this.nextComment = {
            classes: ko.es5.mapping.computed(function () {
                return _this.comments.length > 0 && _this.idxComment < _this.comments.length - 1 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.showCurrentComment(+1);
            },
            display: function () {
                return "";
            }
        };
        this.previousFile = {
            classes: ko.es5.mapping.computed(function () {
                return _this.idxFile >= 1 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.showCurrentFile(_this.idxFile - 1);
            },
            display: function () {
                return "";
            }
        };
        this.nextFile = {
            classes: ko.es5.mapping.computed(function () {
                return _this.idxFile >= 0 && _this.idxFile < _this.viewModel.changeList.changeFiles.length - 1 ? "" : "ui-state-disabled";
            }),
            click: function () {
                _this.showCurrentFile(_this.idxFile + 1);
            },
            display: function () {
                return "";
            }
        };
        this.idxFile = ko.es5.mapping.property(ko.es5.mapping.computed(function () {
            if (_this.viewModel.changeList.file == null)
                return -1;
            return _this.viewModel.changeList.changeFiles.indexOf(_this.viewModel.changeList.file);
        }));
        this.loadChanges = function () {
            var baseTable = $(UI.Revisions.Current.LeftTable).filter(":visible");
            var exclude = baseTable.find('tbody.Unchanged,tbody.Seperator');
            _this.baseChanges = baseTable.find('tbody').not(exclude).filter(":visible");
            _this.baseChanges.removeClass("CurrentChange");

            var diffTable = $(UI.Revisions.Current.RightTable).filter(":visible");
            exclude = diffTable.find('tbody.Unchanged,tbody.Seperator');
            _this.diffChanges = diffTable.find('tbody').not(exclude).filter(":visible");
            _this.diffChanges.removeClass("CurrentChange");
        };
        this.load = function () {
            _this.loadChanges();

            _this.idxChange = -1;
            _this.currentComment = null;

            _this.cookies.load();
            _this.showCurrentChange();

            if (_this.idxChange == 0)
                _this.idxChange--;
        };
        this.reset = function () {
            _this.resetComments();
            _this.resetFiles();
            _this.baseChanges = $();
            _this.diffChanges = $();
            _this.idxChange = -1;
        };
        this.resetComments = function () {
            if (_this.currentComment != null && _this.currentComment.qtipElem != null) {
                var qtip = $(_this.currentComment.qtipElem);
                qtip.removeClass("qtip-CurrentChange");
                qtip.addClass(_this.currentComment.getResource().qtip.classes);
            }

            _this.currentComment = null;
        };
        this.resetFiles = function () {
            _this.currentFile = null;
        };
        this.showCurrentChange = function () {
            _this.loadChanges();
            if (_this.diffChanges.length === 0)
                return;

            if (_this.idxChange < 0)
                _this.idxChange = 0;
else if (_this.idxChange >= _this.diffChanges.length)
                _this.idxChange = _this.diffChanges.length - 1;

            var elem = _this.baseChanges[_this.idxChange];
            $(elem).addClass("CurrentChange");
            scrollToMiddle(UI.Revisions.Current.LeftTable, elem);

            elem = _this.diffChanges[_this.idxChange];
            $(elem).addClass("CurrentChange");
            scrollToMiddle(UI.Revisions.Current.RightTable, elem);
        };
        this.showCurrentComment = function (idx) {
            if (_this.comments.length === 0)
                idx = -1;
else if (idx <= 0)
                idx = 0;
else if (idx + 1 >= _this.comments.length)
                idx = _this.comments.length - 1;

            var nextComment = idx >= 0 ? _this.comments[idx] : null;
            if (nextComment == _this.currentComment)
                return;

            _this.resetComments();
            _this.currentComment = nextComment;
            if (_this.currentComment == null || _this.currentComment.qtipElem == null)
                throw "Could not find comment for qtip";

            var qtip = $(_this.currentComment.qtipElem);
            qtip.removeClass(_this.currentComment.getResource().qtip.classes);
            qtip.addClass("qtip-CurrentChange");

            scrollToMiddle(UI.Revisions.Current.LeftTable, _this.currentComment.placeHolders[0].ux);
            scrollToMiddle(UI.Revisions.Current.RightTable, _this.currentComment.placeHolders[1].ux);
        };
        this.showCurrentFile = function (idx) {
            if (_this.viewModel.changeList.changeFiles.length === 0)
                idx = -1;
else if (idx <= 0)
                idx = 0;
else if (idx + 1 >= _this.viewModel.changeList.changeFiles.length)
                idx = _this.viewModel.changeList.changeFiles.length - 1;

            var next = idx >= 0 ? _this.viewModel.changeList.changeFiles[idx] : null;
            if (next == null || next == _this.currentFile)
                return;

            _this.resetFiles();
            _this.currentFile = next;
            if (_this.currentFile == null)
                throw "Could not find current file";

            _this.viewModel.onFileChangeSelect(next.data.serverFileName);
        };
        this.showUnchangedLinesBody = function (tbody, callback) {
            if (typeof callback === "undefined") { callback = null; }
            callback = callback || function () {
            };
            UI.Effects.hide(tbody, function () {
                var tbodyHidden = tbody.previousElementSibling;
                UI.Effects.show(tbodyHidden, callback);
            });
        };
        this.showUnchangedLines = function (button) {
            var td = getTD(button);
            var tbody = td.parentElement.parentElement;
            _this.showUnchangedLinesBody(tbody);

            var trSrc = td.parentElement;
            var idxSrc = trSrc.rowIndex;

            var tableTgt = UI.Revisions.getPartnerTable(trSrc);
            if (idxSrc >= tableTgt.rows.length)
                return;

            var trTgt = tableTgt.rows[idxSrc];
            if (trTgt == null)
                return;

            var tbody = trTgt.parentElement;
            _this.showUnchangedLinesBody(tbody, function () {
                var hidden = $(UI.Revisions.Current.DiffTable).find("tbody.Unchanged").filter(':hidden').length;
                if (hidden == 0)
                    _this.viewModel.settings.settings.user.hideUnchanged = false;
                _this.viewModel.updatePosition();
            });
        };
        this.viewModel = viewModel;
        this.reset();
        ko.es5.mapping.track(this);

        this.viewModel.events.changed.add(function (type) {
            if (type != ViewModel.DiffEvents.Revision)
                return;

            _this.reset();
            _this.cookies.setName(_this.cookieName());
            _this.load();
            _this.cookies.subscribe();
            setTimeout(function () {
                _this.commentsChanged++;
            }, 100);
            $("input.Seperator").click(function (eventObj) {
                _this.showUnchangedLines(eventObj.target);
            });
        });

        this.viewModel.events.unloaded.add(function (type) {
            _this.cookies.dispose();
            _this.reset();
        });

        this.cookies = new Cookies(this.cookieName(), [
            ko.getObservable(this, "idxChange")
        ]);
    }
    return Navigation;
})();
