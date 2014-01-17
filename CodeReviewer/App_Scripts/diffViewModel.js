/// <reference path="references.ts" />
var ViewModel;
(function (ViewModel) {
    (function (DiffEvents) {
        DiffEvents[DiffEvents["File"] = 1] = "File";
        DiffEvents[DiffEvents["Revision"] = 2] = "Revision";
    })(ViewModel.DiffEvents || (ViewModel.DiffEvents = {}));
    var DiffEvents = ViewModel.DiffEvents;
    var Diff = (function () {
        function Diff() {
            var _this = this;
            this.bound = false;
            this.commentViewModel = new ViewModel.Comment();
            this.comments = ko.es5.mapping.property(function () {
                return _this.changeList.file != null ? _this.changeList.file.comments : [];
            });
            this.displayFilesSelect = ko.es5.mapping.computed(function () {
                if (_this.changeList.changeFiles.length > 1)
                    return "inline";
                return "none";
            });
            this.events = {
                unloaded: $.Callbacks(),
                changed: $.Callbacks()
            };
            this.id = "ViewModel.Diff";
            this.navigation = null;
            this.ping = null;
            this.postBoundCallback = null;
            this.reviewer = null;
            this.reviewers = [];
            this.revision = ko.es5.mapping.computed(function () {
                if (_this.tabIndex < _this.revisions.length)
                    return _this.revisions[_this.tabIndex];
else if (_this.revisions.length > 0) {
                    _this.tabIndex = _this.revisions.length - 1;
                    return _this.revisions[_this.tabIndex];
                }
                return null;
            });
            this.revisions = ko.es5.mapping.property(function () {
                return _this.changeList.file != null ? _this.changeList.file.revisions : [];
            });
            this.select = {
                changeFiles: new UI.select("", [], null, function (newValue) {
                    _this.onFileChangeSelect(newValue);
                })
            };
            this.settings = null;
            this.stage = null;
            this.subscriptions = {
                list: new SubscriptionList(false),
                dispose: function () {
                    _this.subscriptions.list.disposeAll();
                    _this.select.changeFiles.dispose();
                },
                init: function () {
                    _this.subscriptions.list.add(ko.es5.mapping.getObservable(_this, "revision"), function (newValue) {
                        if (newValue != undefined && _this.settings.settings.changeList.fileRevision != newValue.tabIndex) {
                            _this.settings.settings.changeList.fileRevision = newValue.tabIndex;
                        }
                    });
                },
                subscribe: function () {
                    _this.subscriptions.list.subscribeAll();
                    _this.select.changeFiles.subscribe();
                }
            };
            this.tabIndex = 0;
            this.isMe = function () {
                return _this.changeList.data.userName == g_UserName || _this.changeList.data.reviewerAlias == g_ReviewerAlias ? true : false;
            };
            this.postBound = function () {
                var boundCount = 0;
                if (_this.changeList.file == null) {
                    console.log("appViewModel.postBound - this.changeList.file == null");
                    return;
                }
                _this.changeList.file.revisions.forEach(function (revision) {
                    if (revision.bound)
                        boundCount++;
                });
                var revisionCount = _this.changeList.file.revisions.length;
                if (revisionCount != 0 && boundCount < revisionCount)
                    return;

                _this.bound = true;
                if (_this.postBoundCallback != null) {
                    _this.postBoundCallback.resolve(true);
                }
            };
            this.loadComments = function () {
                if (_this.changeList.file == null)
                    return;

                _this.unloadComments();

                // TODO: load in ChangeFile.ts
                // TODO: keep comments / or just the data for re-creation
                _this.changeList.file.data.comments.forEach(function (comment) {
                    var reviewComment = new ReviewComment(comment, _this);
                    if (reviewComment.valid)
                        _this.comments.push(reviewComment);
                });

                // TODO: Replace this.changeList.file.data.comments with this.comments
                // Then change onFileChanged to just loadFileVersions
                _this.commentViewModel.sort();
            };
            this.unloadComments = function () {
                _this.comments.forEach(function (comment) {
                    comment.remove();
                    delete comment;
                });

                //this.comments.removeAll(); // TODO - delete - b/c it really removes the comments
                _this.updatePosition();
            };
            this.loadRevisions = function (callback) {
                if (_this.changeList.file == null)
                    return;

                var requests = _this.changeList.file.loadRevisions();
                if (!_this.bound) {
                    _this.postBoundCallback = $.Deferred();
                    requests.push(_this.postBoundCallback);
                }
                if (requests.length == 0) {
                    _this.postBoundCallback = null;
                    callback();
                } else {
                    $.when.apply(null, requests).done(function () {
                        callback();
                    });
                }
            };
            this.displayRevisions = function () {
                var idxTab = _this.settings.settings.changeList.fileRevision;
                _this.tabIndex = idxTab || 0;

                UI.Revisions.Tabs.load(_this.tabIndex, _this.onTabSelect, _this.onTabVisible);
                $(".DiffTable").click(function (eventObj) {
                    _this.addCommentFromTd(eventObj);
                });
                setTimeout(function () {
                    if (!UI.Revisions.Tabs.visible)
                        UI.Revisions.Tabs.show();
                    UI.Revisions.Tabs.select(_this.tabIndex);
                }, 200);
                UI.onLoad();
                UI.Revisions.onWindowResize();
                if (!UI.Revisions.Tabs.visible()) {
                    UI.Revisions.Tabs.show();
                }
                _this.displayRevisionTabs();
            };
            this.displayRevisionTabs = function () {
                // display revisions tab
                _this.loadComments();
                UI.Revisions.Current.RightTable.scroll(function (event) {
                    _this.comments.forEach(function (comment) {
                        comment.reposition();
                    });
                });
            };
            this.loadReviewers = function () {
                _this.reviewers.removeAll();
                _this.changeList.data.reviewers.forEach(function (reviewer) {
                    _this.reviewers.push(new Reviewer.Person(reviewer));
                });
            };
            this.loadChangeList = function () {
                if (g_ChangeList.changeFiles.length == 0)
                    throw Error("ChangeList is missing files.");

                var curFileName = _this.select.changeFiles.value;
                _this.subscriptions.dispose();

                if (_this.changeList != null)
                    _this.changeList.dispose();
                _this.changeList = new ChangeList(g_ChangeList, _this);

                var list = [];
                var lastFileId = _this.settings.settings.changeList.fileId;
                _this.changeList.changeFiles.forEach(function (changeFile) {
                    // TODO: Check long serverFileName
                    list.push(changeFile.data.serverFileName);
                    if (changeFile.data.id == lastFileId)
                        _this.changeList.file = changeFile;
                });

                // Update changeFiles list with new items
                ko_MergeArrayES5(_this.select.changeFiles.list, list);
                _this.select.changeFiles.value = _this.changeList.file.data.serverFileName;
                _this.settings.settings.changeList.fileId = _this.changeList.file.data.id;
            };
            this.load = function () {
                _this.events.unloaded.fire(DiffEvents.File);
                _this.loadChangeList();
                _this.loadRevisions(function () {
                    _this.displayRevisions();

                    // this.loadComments();
                    _this.loadReviewers();
                    _this.loadComplete();
                });
            };
            this.loadComplete = function () {
                if (_this.changeList.file != null)
                    _this.changeList.file.loaded = true;
                _this.subscriptions.subscribe();
                _this.events.changed.fire(DiffEvents.File);
            };
            this.onFileChangeSelect = function (newValue) {
                if (_this.changeList.file == null || _this.changeList.file.data.serverFileName != newValue) {
                    var curFile = _this.changeList.findChangeFile(newValue);
                    if (curFile != null) {
                        _this.settings.settings.changeList.fileId = curFile.data.id;
                    }
                    _this.onFileChanged(newValue);
                }
            };
            this.onFileChanged = function (newValue) {
                if (_this.changeList.file != null && _this.changeList.file.data.serverFileName == newValue)
                    return;

                _this.events.unloaded.fire(DiffEvents.File);

                _this.bound = false;
                _this.revisions.forEach(function (revision) {
                    revision.bound = false;
                });

                // Update cached changeList with comment data
                _this.commentViewModel.syncComments();

                // defer following to allow UI to update
                setTimeout(function () {
                    //UI.Revisions.Tabs.animation.clear();
                    UI.Revisions.Tabs.hide(function () {
                        // destroy tabs to reload properly
                        UI.Revisions.Tabs.destroy();

                        if (_this.changeList.changeFiles.length == 0)
                            return;

                        _this.subscriptions.dispose();
                        _this.unloadComments();

                        var found = _this.changeList.findChangeFile(newValue);
                        if (found == null) {
                            UI.Error("Did not file", "Could not find file - " + newValue);
                            return;
                        }
                        _this.changeList.file = found;
                        _this.select.changeFiles.value = _this.changeList.file.data.serverFileName;
                        if (found == null) {
                            _this.settings.subscribe();

                            alert("Error missing " + newValue);
                            return;
                        }

                        _this.loadRevisions(function () {
                            _this.displayRevisions();
                            _this.loadComplete();
                        });
                    });
                }, 100);
            };
            this.onTabVisible = function (newIndex) {
                setTimeout(function () {
                    UI.onWindowResize();
                    _this.comments.forEach(function (comment) {
                        comment.move(_this.tabIndex);
                    });
                    _this.updatePosition();
                    _this.events.changed.fire(DiffEvents.Revision);
                }, 200);
                return true;
            };
            this.onTabSelect = function (newIndex) {
                if (_this.tabIndex == newIndex)
                    return true;

                _this.events.unloaded.fire(DiffEvents.Revision);
                _this.tabIndex = newIndex;
                UI.Revisions.onRevisionChange(newIndex);
                UI.Revisions.onWindowResize();

                //UI.Revisions.Tabs.animation.set();
                UI.QTip.showAll(false);
                return true;
            };
            this.addCommentFromTd = function (eventObj) {
                eventObj.stopPropagation();
                var target = eventObj.target;
                var td = getTD(target);
                if (td == null)
                    return;
                var tr = td.parentElement;

                if (tr.className.indexOf("PlaceHolder") != -1 || td.className.indexOf("Seperator") != -1 || td.className.indexOf("CommentLine") != -1) {
                    return;
                }
                if (td.className.indexOf("Code") != -1) {
                    g_ViewModel.commentViewModel.addComment(td);
                }
            };
            this.reposition = function () {
                $('.qtip:visible').qtip('reposition');
            };
            this.updatePosition = function () {
                _this.comments.forEach(function (comment) {
                    comment.updatePosition();
                });

                _this.comments.forEach(function (comment) {
                    comment.calcHeight();
                });

                _this.reposition();
            };
            this.showComment = function (elem) {
                // TODO: Verify this still works
                var found = _this.comments.first(function (comment) {
                    if (comment.edge != null) {
                        return (comment.edge.id == elem.id);
                    }
                    return false;
                });
                if (found == null)
                    return;
                found.createUX();
            };
            this.received = function (data) {
                if (data == null || data.type == null || data.type == undefined)
                    return;

                switch (data.type) {
                    case "delete":
                         {
                            var dto = data;
                            if (dto.delete == null || dto.delete == undefined)
                                return;

                            switch (dto.delete) {
                                case "CommentGroup":
                                    _this.commentViewModel.removeComment(dto.id);
                                    break;

                                case "Reviewer":
                                    _this.reviewer.removeReviewer(dto.id);
                                    break;
                            }
                        }
                        break;

                    case "CommentGroup":
                        if (_this.settings.settings.user.dynamic.comments)
                            _this.commentViewModel.updateComment(data);
                        break;

                    case "ChangeList":
                        break;

                    case "ChangeListStatus":
                        _this.stage.updateData(data);
                        break;

                    case "ChangeFile":
                        break;

                    case "Reviewer":
                        _this.reviewer.updateReviewer(data);
                        break;

                    default:
                        console.log("received - unknown data type", data);
                }
            };
            g_ViewModel = this;
            this.changeList = new ChangeList(g_ChangeList, this);
            this.settings = new ViewModel.Settings(this);

            this.load();

            this.commentViewModel.init(this);
            this.ping = new Ping(this);
            this.reviewer = new Reviewer.Service(this);
            this.stage = new Stage(this);
            this.leftPane = new LeftPane(this.settings.settings.user);
            ko.es5.mapping.track(this);

            this.navigation = new Navigation(this);
            this.subscriptions.init();
        }
        return Diff;
    })();
    ViewModel.Diff = Diff;

    function initDiffViewModel() {
        /* jquery 1-9-1
        jQuery.fn.live = function (types, data, fn) {
        jQuery(this.context).on(types, this.selector, data, fn);
        return this;
        };
        */
        //Logger.initLogger();
        g_ChangeList = getInitialChangeList();
        g_ChangeListSettings = getInitialChangeListSettings();
        g_UserSettings = getInitialUserSettings();

        $.cookie.json = true;

        //$.cookie.defaults = { path: '/', expires: 365 };
        //JSPlumber.Init();
        g_ViewModel = new ViewModel.Diff();
        ko.applyBindings(g_ViewModel);

        window.onresize = UI.onWindowResize;

        //UI.ContextMenu.init();
        var connection = $.connection.changeListHub;

        //$.connection.hub.logging = true;
        connection.client.received = function (data) {
            console.log("received", data.type, data);
            g_ViewModel.received(data);
        };

        $.connection.hub.start().done(function () {
            connection.server.join(g_ChangeList.CL);
        });

        shortcut.add("F3", function () {
            g_ViewModel.navigation.previousFile.click();
        });

        shortcut.add("F4", function () {
            g_ViewModel.navigation.nextFile.click();
        });

        shortcut.add("ESC", function () {
            g_ViewModel.navigation.nextFile.click();
        });

        shortcut.add("F7", function () {
            g_ViewModel.navigation.previousChange.click();
        });

        shortcut.add("F8", function () {
            g_ViewModel.navigation.nextChange.click();
        });

        shortcut.add("F9", function () {
            g_ViewModel.navigation.previousComment.click();
        });

        shortcut.add("F10", function () {
            g_ViewModel.navigation.nextComment.click();
        });
    }
    ViewModel.initDiffViewModel = initDiffViewModel;
})(ViewModel || (ViewModel = {}));

ko.bindingHandlers.accordion = {
    init: function (element, valueAccessor) {
        var options = valueAccessor() || {};
        setTimeout(function () {
            ($(element)).accordion(options);
        }, 0);

        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            ($(element)).accordion("destroy");
        });
    },
    update: function (element, valueAccessor) {
        var options = valueAccessor() || {};
        ($(element)).accordion("destroy").accordion(options);
    }
};

ko.bindingHandlers.viewModelBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        //viewModel.postBound(element);
    }
};

ko.bindingHandlers.revisionBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        //console.log("revisionBind", element);
        viewModel.postBound(element);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called once when the binding is first applied to an element,
        // and again whenever the associated observable changes value.
        // Update the DOM element based on the supplied values here.
        //console.log("revisionBind.update", element);
        /*
        // First get the latest data that we're bound to
        var value = valueAccessor(), allBindings = allBindingsAccessor();
        
        // Next, whether or not the supplied model property is observable, get its current value
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        // console.log(valueUnwrapped);
        
        viewModel.top(viewModel.currentTop());
        */
    }
};
