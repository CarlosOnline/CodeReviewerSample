/// <reference path="references.ts" />

module ViewModel {
    export enum DiffEvents {
        File = 1,
        Revision = 2,
    }
    export class Diff {

        bound = false;
        changeList: ChangeList;
        commentViewModel = new Comment();
        comments = ko.es5.mapping.property<Array<ReviewComment>>(() => { return this.changeList.file != null ? this.changeList.file.comments : [] });
        displayFilesSelect = ko.es5.mapping.computed<string>(() => {
            if (this.changeList.changeFiles.length > 1)
                return "inline";
            return "none";
        });
        events = {

            unloaded: $.Callbacks(),
            changed: $.Callbacks(),
        };
        id = "ViewModel.Diff";
        leftPane: LeftPane;
        navigation = null;
        ping: Ping = null;
        postBoundCallback: JQueryDeferred<boolean> = null;
        reviewer: Reviewer.Service = null;
        reviewers: Array<Reviewer.Person> = [];
        revision = ko.es5.mapping.computed<Revision>(() => {
            if (this.tabIndex < this.revisions.length)
                return this.revisions[this.tabIndex];
            else if (this.revisions.length > 0) {
                this.tabIndex = this.revisions.length - 1;
                return this.revisions[this.tabIndex];
            }
            return null;
        });
        revisions = ko.es5.mapping.property<Array<Revision>>(() => { return this.changeList.file != null ? this.changeList.file.revisions : []; });

        select = {
            changeFiles: new UI.select("", [], null, (newValue) => { this.onFileChangeSelect(newValue) }),
        };

        settings: Settings = null;
        stage: Stage = null;
        subscriptions = {
            list: new SubscriptionList(false),

            dispose: () => {
                this.subscriptions.list.disposeAll();
                this.select.changeFiles.dispose();
            },

            init: () => {
                this.subscriptions.list.add(ko.es5.mapping.getObservable(this, "revision"), (newValue) => {
                    if (newValue != undefined && this.settings.settings.changeList.fileRevision != newValue.tabIndex) {
                        this.settings.settings.changeList.fileRevision = newValue.tabIndex;
                    }
                });
            },

            subscribe: () => {
                this.subscriptions.list.subscribeAll();
                this.select.changeFiles.subscribe();
            },
        };
        tabIndex = 0;

        constructor() {
            g_ViewModel = this;
            this.changeList = new ChangeList(g_ChangeList, this); // TODO: remo fix load
            this.settings = new Settings(this);

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

        isMe = () => {
            return this.changeList.data.userName == g_UserName || this.changeList.data.reviewerAlias == g_ReviewerAlias ? true : false;
        };

        postBound = () => {
            var boundCount = 0;
            if (this.changeList.file == null) {
                console.log("appViewModel.postBound - this.changeList.file == null");
                return;
            }
            this.changeList.file.revisions.forEach((revision) => {
                if (revision.bound)
                    boundCount++;
            });
            var revisionCount = this.changeList.file.revisions.length;
            if (revisionCount != 0 && boundCount < revisionCount)
                return;

            this.bound = true;
            if (this.postBoundCallback != null) {
                this.postBoundCallback.resolve(true);
            }
        };

        loadComments = () => {
            if (this.changeList.file == null)
                return;

            this.unloadComments();

            // TODO: load in ChangeFile.ts
            // TODO: keep comments / or just the data for re-creation
            this.changeList.file.data.comments.forEach((comment) => {
                var reviewComment = new ReviewComment(comment, this);
                if (reviewComment.valid)
                    this.comments.push(reviewComment);
            });
            // TODO: Replace this.changeList.file.data.comments with this.comments
            // Then change onFileChanged to just loadFileVersions

            this.commentViewModel.sort();
        };

        unloadComments = () => {
            this.comments.forEach((comment) => {
                comment.remove();
                delete comment; // TODO?
            });
            //this.comments.removeAll(); // TODO - delete - b/c it really removes the comments
            this.updatePosition();
        };

        loadRevisions = (callback: Function) => {
            if (this.changeList.file == null)
                return;

            var requests = this.changeList.file.loadRevisions();
            if (!this.bound) {
                this.postBoundCallback = $.Deferred();
                requests.push(this.postBoundCallback);
            }
            if (requests.length == 0) {
                this.postBoundCallback = null;
                callback();
            } else {
                $.when.apply(null, requests).done(() => {
                    callback();
                });
            }
        };

        displayRevisions = () => {
            var idxTab = this.settings.settings.changeList.fileRevision;
            this.tabIndex = idxTab || 0;

            UI.Revisions.Tabs.load(this.tabIndex, this.onTabSelect, this.onTabVisible);
            $(".DiffTable").click((eventObj: JQueryEventObject) => {
                this.addCommentFromTd(eventObj);
            });
            setTimeout(() => {
                if (!UI.Revisions.Tabs.visible)
                    UI.Revisions.Tabs.show();
                UI.Revisions.Tabs.select(this.tabIndex);
            }, 200);
            UI.onLoad();
            UI.Revisions.onWindowResize();
            if (!UI.Revisions.Tabs.visible()) {
                UI.Revisions.Tabs.show();
            }
            this.displayRevisionTabs();
        };

        displayRevisionTabs = () => {
            // display revisions tab
            this.loadComments();
            UI.Revisions.Current.RightTable.scroll((event) => {
                this.comments.forEach((comment) => {
                    comment.reposition();
                });
            });
        };

        loadReviewers = () => {
            this.reviewers.removeAll();
            this.changeList.data.reviewers.forEach((reviewer) => {
                this.reviewers.push(new Reviewer.Person(reviewer));
            });
        };

        loadChangeList = () => {
            if (g_ChangeList.changeFiles.length == 0)
                throw Error("ChangeList is missing files.");

            var curFileName = this.select.changeFiles.value;
            this.subscriptions.dispose();

            if (this.changeList != null)
                this.changeList.dispose();
            this.changeList = new ChangeList(g_ChangeList, this);

            var list = [];
            var lastFileId = this.settings.settings.changeList.fileId;
            this.changeList.changeFiles.forEach((changeFile) => {
                // TODO: Check long serverFileName
                list.push(changeFile.data.serverFileName);
                if (changeFile.data.id == lastFileId)
                    this.changeList.file = changeFile;
            });

            // Update changeFiles list with new items
            ko_MergeArrayES5(this.select.changeFiles.list, list);
            this.select.changeFiles.value = this.changeList.file.data.serverFileName;
            this.settings.settings.changeList.fileId = this.changeList.file.data.id;
        };

        load = () => {
            this.events.unloaded.fire(DiffEvents.File);
            this.loadChangeList();
            this.loadRevisions(() => {
                this.displayRevisions();
                // this.loadComments();
                this.loadReviewers();
                this.loadComplete();
            });
        };

        loadComplete = () => {
            if (this.changeList.file != null)
                this.changeList.file.loaded = true;
            this.subscriptions.subscribe();
            this.events.changed.fire(DiffEvents.File);
        };

        onFileChangeSelect = (newValue: string) => {
            if (this.changeList.file == null ||
                this.changeList.file.data.serverFileName != newValue) {
                var curFile = this.changeList.findChangeFile(newValue);
                if (curFile != null) {
                    this.settings.settings.changeList.fileId = curFile.data.id;
                }
                this.onFileChanged(newValue);
            }
        };

        onFileChanged = (newValue: string) => {
            if (this.changeList.file != null && this.changeList.file.data.serverFileName == newValue)
                return;

            this.events.unloaded.fire(DiffEvents.File);

            this.bound = false;
            this.revisions.forEach((revision) => {
                revision.bound = false;
            });

            // Update cached changeList with comment data
            this.commentViewModel.syncComments();

            // defer following to allow UI to update
            setTimeout(() => {
                //UI.Revisions.Tabs.animation.clear();
                UI.Revisions.Tabs.hide(() => {
                    // destroy tabs to reload properly
                    UI.Revisions.Tabs.destroy();

                    if (this.changeList.changeFiles.length == 0)
                        return;

                    this.subscriptions.dispose();
                    this.unloadComments();

                    var found = this.changeList.findChangeFile(newValue);
                    if (found == null) {
                        UI.Error("Did not file", "Could not find file - " + newValue);
                        return;
                    }
                    this.changeList.file = found;
                    this.select.changeFiles.value = this.changeList.file.data.serverFileName;
                    if (found == null) {
                        this.settings.subscribe();

                        alert("Error missing " + newValue);
                        return;
                    }

                    this.loadRevisions(() => {
                        this.displayRevisions();
                        this.loadComplete();
                    });
                });
            }, 100);
        };

        onTabVisible = (newIndex: number) => {
            setTimeout(() => {
                UI.onWindowResize();
                this.comments.forEach((comment) => {
                    comment.move(this.tabIndex);
                });
                this.updatePosition();
                this.events.changed.fire(DiffEvents.Revision);
            }, 200);
            return true;
        };

        onTabSelect = (newIndex: number) => {
            if (this.tabIndex == newIndex)
                return true;

            this.events.unloaded.fire(DiffEvents.Revision);
            this.tabIndex = newIndex;
            UI.Revisions.onRevisionChange(newIndex);
            UI.Revisions.onWindowResize();
            //UI.Revisions.Tabs.animation.set();

            UI.QTip.showAll(false);
            return true;
        };

        addCommentFromTd = (eventObj: JQueryEventObject) => {
            eventObj.stopPropagation();
            var target = <HTMLElement> eventObj.target;
            var td = getTD(target);
            if (td == null)
                return;
            var tr = td.parentElement;

            if (tr.className.indexOf("PlaceHolder") != -1 ||
                td.className.indexOf("Seperator") != -1 ||
                td.className.indexOf("CommentLine") != -1) {
                return;
            }
            if (td.className.indexOf("Code") != -1) {
                g_ViewModel.commentViewModel.addComment(td);
            }
        }

        reposition = () => {
            $('.qtip:visible').qtip('reposition');
        };

        updatePosition = () => {
            this.comments.forEach((comment) => {
                comment.updatePosition();
            });

            this.comments.forEach((comment) => {
                comment.calcHeight();
            });

            this.reposition();
        };

        showComment = (elem: HTMLElement) => {
            // TODO: Verify this still works
            var found = this.comments.first((comment) => {
                if (comment.edge != null) {
                    return (comment.edge.id == elem.id);
                }
                return false;
            });
            if (found == null)
                return;
            found.createUX();
        };

        received = (data) => {
            if (data == null || data.type == null || data.type == undefined)
                return;

            switch (data.type) {
                case "delete":
                    {
                        var dto: Dto.DeleteNotification = data;
                        if (dto.delete == null || dto.delete == undefined)
                            return;

                        switch (dto.delete) {
                            case "CommentGroup":
                                this.commentViewModel.removeComment(dto.id);
                                break;

                            case "Reviewer":
                                this.reviewer.removeReviewer(dto.id);
                                break;
                        }
                    }
                    break;

                case "CommentGroup":
                    if (this.settings.settings.user.dynamic.comments)
                        this.commentViewModel.updateComment(<Dto.CommentGroupDto> data);
                    break;

                case "ChangeList":
                    break;

                case "ChangeListStatus":
                    this.stage.updateData(<Dto.ChangeListStatusDto> data);
                    break;

                case "ChangeFile":
                    break;

                case "Reviewer":
                    this.reviewer.updateReviewer(<Dto.ReviewerDto> data);
                    break;

                default:
                    console.log("received - unknown data type", data);
            }
        };
    } // App class

    export function initDiffViewModel() {
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

} // ViewModel module

interface KnockoutBindingHandlers {
    accordion: KnockoutBindingHandler; // TODO: Remove
    revisionBind: KnockoutBindingHandler;
    viewModelBind: KnockoutBindingHandler;
}

ko.bindingHandlers.accordion = {
    init: function (element, valueAccessor) {
        var options = valueAccessor() || {};
        setTimeout(function () {
            (<any>$(element)).accordion(options);
        }, 0);

        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            (<any>$(element)).accordion("destroy");
        });
    },
    update: function (element, valueAccessor) {
        var options = valueAccessor() || {};
        (<any>$(element)).accordion("destroy").accordion(options);
    }
};

ko.bindingHandlers.viewModelBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        //viewModel.postBound(element);
    }
    /*
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called once when the binding is first applied to an element,
        // and again whenever the associated observable changes value.
        // Update the DOM element based on the supplied values here.
        // console.log(element);

        // First get the latest data that we're bound to
        var value = valueAccessor(), allBindings = allBindingsAccessor();

        // Next, whether or not the supplied model property is observable, get its current value
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        // console.log(valueUnwrapped);

        viewModel.top(viewModel.currentTop());
    }*/
}

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