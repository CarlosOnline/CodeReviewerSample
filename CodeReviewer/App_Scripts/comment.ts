/// <reference path="references.ts" />

class ReviewComment {
    cookies: Cookies = null;
    data: Dto.CommentGroupDto;
    defaultHeight = 50;
    edge: CommentEdge = null;
    height = 50;

    hidden = false;
    public id: string;
    public line = {
        id: "",
        info: <any> null, // TODO: Map to LineStampInfo type
        rowId: "",
        table: <HTMLTableElement> null,
        td: <HTMLTableCellElement> null,
        top: <KnockoutObservable<string>> null,
        tr: <HTMLTableRowElement> null,
    };
    lineStamp: string;
    placeHolders: Array<PlaceHolderComment> = [];
    qtipElem: HTMLElement = null;
    rowId: string;
    subscriptions = new SubscriptionList();
    revision = 0;
    threads: Array<CommentThread> = [];
    ux: HTMLElement = null;
    valid = false;
    viewModel: ViewModel.Diff = null;

    display = {
        comment: ko.es5.mapping.computed(() => { return this.visible ? "inline" : "none" }),
        addThread: ko.es5.mapping.computed(() => {
            var len = this.threads.length;
            if (len == 0)
                return "inline";
            return this.threads[len - 1].display.viewer;
        }),
        status: ko.es5.mapping.computed(() => {
            return this.threads.length == 0 || this.threads[0].data.id == 0 ? "none" : "inline";
        }),
        hideGroup: ko.es5.mapping.computed(() => {
            return this.threads.length == 0 || this.threads[0].data.id == 0 ? "none" : "inline";
        }),
    };

    status = {
        select: new UI.select(Resources.Comment.Status.value, Resources.Comment.Status.list, null, (newValue) => { this.onStatusChange(newValue); }),
        color: ko.es5.mapping.computed(() => {
            return this.getResource().key;
        }),
        qtip: {
            classes: ko.es5.mapping.computed(() => {
                return this.getResource().key;
            }),
        },
        icon: {
            classes: ko.es5.mapping.computed(() => {
                return this.getResource().icon.classes;
            }),
        },
    };


    addQtip = () => {
        if (this.qtipElem != null)
            return;

        UI.QTip.add(this.line.td, this.ux, this.status.select.value,
            (event, api) => {
                var qtip = document.getElementById("qtip-" + api.id);
                this.qtipElem = qtip;
                UI.QTip.setClass(this.qtipElem, this.getResource().qtip.classes);
                this.calcHeight();
                g_ViewModel.updatePosition();
                $(this.ux).qtip("show");
            },
            (event, api) => {
                this.calcHeight();
                g_ViewModel.updatePosition();
                UI.QTip.set(this.qtipElem, { "position.adjust.y": - $(this.line.td).height() });
            });
    };

    addNewThread = () => {
        var data = new Dto.CommentDto();
        data.userName = g_UserName;
        data.reviewerAlias = g_ReviewerAlias;
        data.reviewRevision = g_ViewModel.revision.data.reviewRevision;
        data.fileVersionId = g_ViewModel.revision.data.id;
        data.groupId = this.data.id;
        this.threads.push(new CommentThread(data, this));
        return false;
    };

    calcHeight = (updatePosition = false) => {
        var qtipPadding = $(this.line.td).height() / 2;
        var elem = this.qtipElem;
        if ((elem || null) == null)
            return this.defaultHeight;
        var height = $(elem).height() + qtipPadding;
        if (this.height == height)
            return this.height;

        this.height = $(elem).height() + qtipPadding;

        this.resizePlaceholders();
        this.reposition();

        if (updatePosition)
            g_ViewModel.updatePosition();

        return $(elem).height();
    };

    cancel = () => { return false; };

    close = () => {
        if (this.threads.length == 1) {
            // Remove comment if it's new and blank
            var thread = this.threads[0];
            if (thread.data.id == 0 && thread.data.commentText.length == 0) {
                this.threads.remove(thread);
                return;
            }
        }

        this.hidden = true;
        this.remove(true);

        // Need to use absolute or something table doesnt work
        if (this.edge == null) {
            var edge = new CommentEdge(this);
            this.edge = edge;
        }
        this.edge.show();

        // TODO: Make work
        //$(this.qtipElem).effect("transfer", { to: $(this.edge.ux()) }, 500);
        return false;
    };

    createUX = () => {
        if (this.ux != null)
            throw Error("comment ui has already been created");

        // dynamic creation to allow proper binding
        // otherwise knockout wont bind it properly

        if (this.hidden) {
            // unhide threads if needed
            var found = this.threads.first((thread) => {
                return thread.show == true;
            });
            var show = found != null;

            if (!show) {
                this.threads.forEach((thread) => {
                    thread.show = true;
                });
            }

            this.hidden = false;
        }

        if (this.edge != null) {
            this.edge.remove();
        }

        this.ux = UI.createElement("commentTemplate", this.id);
        ko.applyBindings(this, this.ux);
    };

    dispose = () => {
        $(this.line.tr).removeClass("CommentLine");

        this.subscriptions.disposeAll();

        g_ViewModel.comments.remove(this);
        this.cookies.remove();

        while (this.placeHolders.length > 0) {
            var holder = this.placeHolders.pop();
            holder.remove();
            delete holder;
        }

        while (this.threads.length > 0) {
            var thread = this.threads.pop();
            thread.remove();
            delete thread;
        }

        if (this.edge != null) {
            this.edge.remove();
            delete this.edge;
            this.edge = null;
        }
        this.remove(true);
    };

    displayWhenReady = () => {
        if (!this.hidden) {
            this.createUX();
        } else {
            this.close();
        }
    };

    findThread = (id: number) => {
        var found = this.threads.first((thread: CommentThread) => {
            if (thread != null) {
                if (thread.data.id == id)
                    return true;
            }
            return false;
        });
        return found;
    };

    findDataThread = (threads: Array<Dto.CommentDto>, id: number) => {
        var found = null;
        threads.forEach((thread) => {
            if (thread.id == id) {
                found = thread;
                return thread;
            }
        });
        return found;
    };

    getResource = () => {
        return Resource.CommentStatus.getResource(this.status.select.value);
    };

    hideComment = () => {
        $(this.line.tr).removeClass("CommentLine");

        if (this.qtipElem != null) {
            var api = $(this.qtipElem).qtip('api');
            if (api != null)
                api.hide();
        }

        this.placeHolders.forEach((placeHolder) => {
            placeHolder.hide();
        });
    };

    init = () => {
        var td = this.viewModel.commentViewModel.getTD(this.data.lineStamp);
        this.lineStamp = td != null ? td.id : this.data.lineStamp;
        if (td == null) {
            this.remove();
            g_ViewModel.comments.remove(this);
            return false;
        }

        this.id = "Comment_" + this.lineStamp;
        this.rowId = "CommentRow_" + this.lineStamp;
        this.line.td = <HTMLTableCellElement> td;
        this.line.tr = <HTMLTableRowElement> td.parentElement;
        this.line.table = getElement(this.line.tr, "table");
        this.line.top = ko.observable(this.line.tr.style.top);
        this.line.rowId = this.line.tr.id;
        this.line.info = ViewModel.getLineStampInfo(this.line.td.id);
        this.revision = this.viewModel.tabIndex;

        $(this.line.tr).addClass("CommentLine");
        this.valid = true;

        return true;
    };

    initSubscriptions = () => {
        this.status.select.subscribe();
        this.cookies = new Cookies("comment-" + this.data.id, [
            ko.es5.mapping.getObservable(this, "hidden"),
        ], true, true);

        this.subscriptions.add(ko.es5.mapping.getObservable(this, "threads"), () => {
            if (this.threads.length == 0) {
                if (this.data.id != 0) {
                    AJAX.deleteComment(this.data.id);
                    this.cookies.remove();
                }
                this.dispose();
            } else if (this.data.id == 0) {
                // this.threadChanged(this.threads[0]);
            } else {
                // thread added or removed - do nothing
            }
            g_ViewModel.updatePosition();
        });

        this.subscriptions.add(ko.es5.mapping.getObservable(this.status.qtip, "classes"), () => {
            UI.QTip.setClass(this.qtipElem, this.status.qtip.classes);
        });
    };

    move = (revision: number) => {
        if (revision == this.revision) {
            this.reposition();
            return;
        }
        var td = this.viewModel.commentViewModel.getTD(this.data.lineStamp);

        this.remove(true);
        if (td == null)
            return;

        this.init();
        this.displayWhenReady();
    };

    onStatusChange = (newValue: string) => {
        this.data.status = this.status.select.idx;
        if (this.qtipElem != null) {
            $(this.line.td).qtip('option', {
                'style.classes': this.getResource().qtip.classes
            });
        }

        AJAX.addComment(this, (newData: Dto.CommentGroupDto) => {
            this.updateData(newData);
        });
    };

    postBound = (element: HTMLElement) => {
        this.ux = element;
        this.ux.id = this.id;
        this.addQtip();

        // add placeHolders for each table
        this.placeHolders.push(new PlaceHolderComment(this, UI.Revisions.Current.Tables.LeftTable[0]));
        this.placeHolders.push(new PlaceHolderComment(this, UI.Revisions.Current.Tables.RightTable[0]));
    };

    remove = (all = false) => {
        $(this.line.tr).removeClass("CommentLine");

        if (this.qtipElem != null) {
            var api = $(this.qtipElem).qtip('api');
            if (api != null)
                api.destroy(true);
            $(this.qtipElem).remove();
            this.qtipElem = null;
        }

        this.placeHolders.forEach((placeHolder) => {
            placeHolder.remove();
        });
        this.placeHolders.removeAll();

        if (all) {
            if (this.edge != null) {
                this.edge.remove();
                delete this.edge;
                this.edge = null;
            }

            if (this.ux != null) {
                this.ux.id = "Deleted-" + this.ux.id;
                this.unbind();
                $(this.ux).remove();
                this.ux = null;
            }
        }
    };

    reposition = () => {
        if (this.edge != null) {
            this.edge.reposition();
        }
    };

    resizePlaceholders = () => {
        this.placeHolders.forEach((placeHolder) => {
            $(placeHolder.ux).height(this.height);
        });

        $(this.id).qtip("updatePosition");
    };

    show = () => {
        if (this.ux == null) {
            this.createUX();
            return;
        }
        if (this.edge != null) {
            this.unbind();
            ko.applyBindings(this, this.ux);
        }
        this.threads.forEach((thread) => {
            thread.show = true;
        });
        if (this.hidden)
            this.hidden = false;
        if (this.edge != null) {
            this.edge.remove();
        }
        this.addQtip();
        this.placeHolders.forEach((placeHolder) => {
            $(placeHolder.ux).show(0);
        });
        g_ViewModel.updatePosition();
    };

    scrollIntoView = () => {
        this.line.td.scrollIntoView();
    };

    threadService = {
        changed: (thread: CommentThread) => {
            AJAX.addThread(thread, (newThread: Dto.CommentDto) => {
                newThread = ko.es5.mapping.track(newThread);
                if (this.data.id == 0)
                    this.data.id = newThread.groupId;
                thread.updateData(newThread);
                this.calcHeight();
                g_ViewModel.updatePosition();
            });
        },

        deleted: (thread: CommentThread) => {
            AJAX.deleteThread(thread, (newData: any) => {
                this.calcHeight();
                g_ViewModel.updatePosition();
            });
        },
    };

    unbind = () => {
        ko.unapplyBindings($(this.ux), true);
        this.placeHolders.forEach((placeHolder) => {
            placeHolder.unbind();
        });
    };

    updateData = (newData: Dto.CommentGroupDto) => {
        this.data.status = newData.status;

        // update & add new threads
        newData.threads.forEach((newThreadData) => {
            var found = this.findThread(newThreadData.id);
            if (found) {
                found.updateData(newThreadData);
            } else {
                this.threads.push(new CommentThread(newThreadData, this));
            }
        });

        // get threads no longer in db
        var missing = [];
        this.threads.forEach((thread: CommentThread) => {
            var found = this.findDataThread(newData.threads, thread.data.id);
            if (found == null)
                missing.push(thread);
        });

        // remove threads no longer in database
        missing.forEach((thread: CommentThread) => {
            this.threads.remove(thread);
        });
    };

    updatePosition = () => {
        if (this.line.top() != this.line.tr.style.top)
            this.line.top(this.line.tr.style.top);
    };

    visible = ko.es5.mapping.computed(() => {
        if (this.threads.length == 0 || this.hidden)
            return false;

        var visible = this.threads.first((thread) => {
            return thread.show;
        });
        return (visible != null) ? true : false;
    });

    constructor(comment: Dto.CommentGroupDto, viewModel: ViewModel.Diff, newComment =  false) {
        this.viewModel = viewModel;
        this.data = comment; // already mapped from ChangeFile object
        this.init();
        this.status.select.idx = this.data.status;

        // Initialize Comment
        if (this.valid) {
            $(this.line.td).addClass("CommentLine");
            this.data.threads.forEach((thread) => {
                this.threads.push(new CommentThread(thread, this));
            });

            ko.es5.mapping.track(this);
            this.initSubscriptions();
            if (newComment) {
                // erase previous cookie when a comment is re-created
                this.cookies.remove();
            }
            this.cookies.load();
            this.displayWhenReady();
        }
    }
}

interface KnockoutBindingHandlers {
    commentBind: KnockoutBindingHandler;
}

ko.bindingHandlers.commentBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        viewModel.postBound(element);
    }
};
