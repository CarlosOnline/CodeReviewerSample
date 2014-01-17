/// <reference path="references.ts" />
var ReviewComment = (function () {
    function ReviewComment(comment, viewModel, newComment) {
        if (typeof newComment === "undefined") { newComment = false; }
        var _this = this;
        this.cookies = null;
        this.defaultHeight = 50;
        this.edge = null;
        this.height = 50;
        this.hidden = false;
        this.line = {
            id: "",
            info: null,
            rowId: "",
            table: null,
            td: null,
            top: null,
            tr: null
        };
        this.placeHolders = [];
        this.qtipElem = null;
        this.subscriptions = new SubscriptionList();
        this.revision = 0;
        this.threads = [];
        this.ux = null;
        this.valid = false;
        this.viewModel = null;
        this.display = {
            comment: ko.es5.mapping.computed(function () {
                return _this.visible ? "inline" : "none";
            }),
            addThread: ko.es5.mapping.computed(function () {
                var len = _this.threads.length;
                if (len == 0)
                    return "inline";
                return _this.threads[len - 1].display.viewer;
            }),
            status: ko.es5.mapping.computed(function () {
                return _this.threads.length == 0 || _this.threads[0].data.id == 0 ? "none" : "inline";
            }),
            hideGroup: ko.es5.mapping.computed(function () {
                return _this.threads.length == 0 || _this.threads[0].data.id == 0 ? "none" : "inline";
            })
        };
        this.status = {
            select: new UI.select(Resources.Comment.Status.value, Resources.Comment.Status.list, null, function (newValue) {
                _this.onStatusChange(newValue);
            }),
            color: ko.es5.mapping.computed(function () {
                return _this.getResource().key;
            }),
            qtip: {
                classes: ko.es5.mapping.computed(function () {
                    return _this.getResource().key;
                })
            },
            icon: {
                classes: ko.es5.mapping.computed(function () {
                    return _this.getResource().icon.classes;
                })
            }
        };
        this.addQtip = function () {
            if (_this.qtipElem != null)
                return;

            UI.QTip.add(_this.line.td, _this.ux, _this.status.select.value, function (event, api) {
                var qtip = document.getElementById("qtip-" + api.id);
                _this.qtipElem = qtip;
                UI.QTip.setClass(_this.qtipElem, _this.getResource().qtip.classes);
                _this.calcHeight();
                g_ViewModel.updatePosition();
                $(_this.ux).qtip("show");
            }, function (event, api) {
                _this.calcHeight();
                g_ViewModel.updatePosition();
                UI.QTip.set(_this.qtipElem, { "position.adjust.y": -$(_this.line.td).height() });
            });
        };
        this.addNewThread = function () {
            var data = new Dto.CommentDto();
            data.userName = g_UserName;
            data.reviewerAlias = g_ReviewerAlias;
            data.reviewRevision = g_ViewModel.revision.data.reviewRevision;
            data.fileVersionId = g_ViewModel.revision.data.id;
            data.groupId = _this.data.id;
            _this.threads.push(new CommentThread(data, _this));
            return false;
        };
        this.calcHeight = function (updatePosition) {
            if (typeof updatePosition === "undefined") { updatePosition = false; }
            var qtipPadding = $(_this.line.td).height() / 2;
            var elem = _this.qtipElem;
            if ((elem || null) == null)
                return _this.defaultHeight;
            var height = $(elem).height() + qtipPadding;
            if (_this.height == height)
                return _this.height;

            _this.height = $(elem).height() + qtipPadding;

            _this.resizePlaceholders();
            _this.reposition();

            if (updatePosition)
                g_ViewModel.updatePosition();

            return $(elem).height();
        };
        this.cancel = function () {
            return false;
        };
        this.close = function () {
            if (_this.threads.length == 1) {
                // Remove comment if it's new and blank
                var thread = _this.threads[0];
                if (thread.data.id == 0 && thread.data.commentText.length == 0) {
                    _this.threads.remove(thread);
                    return;
                }
            }

            _this.hidden = true;
            _this.remove(true);

            if (_this.edge == null) {
                var edge = new CommentEdge(_this);
                _this.edge = edge;
            }
            _this.edge.show();

            // TODO: Make work
            //$(this.qtipElem).effect("transfer", { to: $(this.edge.ux()) }, 500);
            return false;
        };
        this.createUX = function () {
            if (_this.ux != null)
                throw Error("comment ui has already been created");

            if (_this.hidden) {
                // unhide threads if needed
                var found = _this.threads.first(function (thread) {
                    return thread.show == true;
                });
                var show = found != null;

                if (!show) {
                    _this.threads.forEach(function (thread) {
                        thread.show = true;
                    });
                }

                _this.hidden = false;
            }

            if (_this.edge != null) {
                _this.edge.remove();
            }

            _this.ux = UI.createElement("commentTemplate", _this.id);
            ko.applyBindings(_this, _this.ux);
        };
        this.dispose = function () {
            $(_this.line.tr).removeClass("CommentLine");

            _this.subscriptions.disposeAll();

            g_ViewModel.comments.remove(_this);
            _this.cookies.remove();

            while (_this.placeHolders.length > 0) {
                var holder = _this.placeHolders.pop();
                holder.remove();
                delete holder;
            }

            while (_this.threads.length > 0) {
                var thread = _this.threads.pop();
                thread.remove();
                delete thread;
            }

            if (_this.edge != null) {
                _this.edge.remove();
                delete _this.edge;
                _this.edge = null;
            }
            _this.remove(true);
        };
        this.displayWhenReady = function () {
            if (!_this.hidden) {
                _this.createUX();
            } else {
                _this.close();
            }
        };
        this.findThread = function (id) {
            var found = _this.threads.first(function (thread) {
                if (thread != null) {
                    if (thread.data.id == id)
                        return true;
                }
                return false;
            });
            return found;
        };
        this.findDataThread = function (threads, id) {
            var found = null;
            threads.forEach(function (thread) {
                if (thread.id == id) {
                    found = thread;
                    return thread;
                }
            });
            return found;
        };
        this.getResource = function () {
            return Resource.CommentStatus.getResource(_this.status.select.value);
        };
        this.hideComment = function () {
            $(_this.line.tr).removeClass("CommentLine");

            if (_this.qtipElem != null) {
                var api = $(_this.qtipElem).qtip('api');
                if (api != null)
                    api.hide();
            }

            _this.placeHolders.forEach(function (placeHolder) {
                placeHolder.hide();
            });
        };
        this.init = function () {
            var td = _this.viewModel.commentViewModel.getTD(_this.data.lineStamp);
            _this.lineStamp = td != null ? td.id : _this.data.lineStamp;
            if (td == null) {
                _this.remove();
                g_ViewModel.comments.remove(_this);
                return false;
            }

            _this.id = "Comment_" + _this.lineStamp;
            _this.rowId = "CommentRow_" + _this.lineStamp;
            _this.line.td = td;
            _this.line.tr = td.parentElement;
            _this.line.table = getElement(_this.line.tr, "table");
            _this.line.top = ko.observable(_this.line.tr.style.top);
            _this.line.rowId = _this.line.tr.id;
            _this.line.info = ViewModel.getLineStampInfo(_this.line.td.id);
            _this.revision = _this.viewModel.tabIndex;

            $(_this.line.tr).addClass("CommentLine");
            _this.valid = true;

            return true;
        };
        this.initSubscriptions = function () {
            _this.status.select.subscribe();
            _this.cookies = new Cookies("comment-" + _this.data.id, [
                ko.es5.mapping.getObservable(_this, "hidden")
            ], true, true);

            _this.subscriptions.add(ko.es5.mapping.getObservable(_this, "threads"), function () {
                if (_this.threads.length == 0) {
                    if (_this.data.id != 0) {
                        AJAX.deleteComment(_this.data.id);
                        _this.cookies.remove();
                    }
                    _this.dispose();
                } else if (_this.data.id == 0) {
                    // this.threadChanged(this.threads[0]);
                } else {
                    // thread added or removed - do nothing
                }
                g_ViewModel.updatePosition();
            });

            _this.subscriptions.add(ko.es5.mapping.getObservable(_this.status.qtip, "classes"), function () {
                UI.QTip.setClass(_this.qtipElem, _this.status.qtip.classes);
            });
        };
        this.move = function (revision) {
            if (revision == _this.revision) {
                _this.reposition();
                return;
            }
            var td = _this.viewModel.commentViewModel.getTD(_this.data.lineStamp);

            _this.remove(true);
            if (td == null)
                return;

            _this.init();
            _this.displayWhenReady();
        };
        this.onStatusChange = function (newValue) {
            _this.data.status = _this.status.select.idx;
            if (_this.qtipElem != null) {
                $(_this.line.td).qtip('option', {
                    'style.classes': _this.getResource().qtip.classes
                });
            }

            AJAX.addComment(_this, function (newData) {
                _this.updateData(newData);
            });
        };
        this.postBound = function (element) {
            _this.ux = element;
            _this.ux.id = _this.id;
            _this.addQtip();

            // add placeHolders for each table
            _this.placeHolders.push(new PlaceHolderComment(_this, UI.Revisions.Current.Tables.LeftTable[0]));
            _this.placeHolders.push(new PlaceHolderComment(_this, UI.Revisions.Current.Tables.RightTable[0]));
        };
        this.remove = function (all) {
            if (typeof all === "undefined") { all = false; }
            $(_this.line.tr).removeClass("CommentLine");

            if (_this.qtipElem != null) {
                var api = $(_this.qtipElem).qtip('api');
                if (api != null)
                    api.destroy(true);
                $(_this.qtipElem).remove();
                _this.qtipElem = null;
            }

            _this.placeHolders.forEach(function (placeHolder) {
                placeHolder.remove();
            });
            _this.placeHolders.removeAll();

            if (all) {
                if (_this.edge != null) {
                    _this.edge.remove();
                    delete _this.edge;
                    _this.edge = null;
                }

                if (_this.ux != null) {
                    _this.ux.id = "Deleted-" + _this.ux.id;
                    _this.unbind();
                    $(_this.ux).remove();
                    _this.ux = null;
                }
            }
        };
        this.reposition = function () {
            if (_this.edge != null) {
                _this.edge.reposition();
            }
        };
        this.resizePlaceholders = function () {
            _this.placeHolders.forEach(function (placeHolder) {
                $(placeHolder.ux).height(_this.height);
            });

            $(_this.id).qtip("updatePosition");
        };
        this.show = function () {
            if (_this.ux == null) {
                _this.createUX();
                return;
            }
            if (_this.edge != null) {
                _this.unbind();
                ko.applyBindings(_this, _this.ux);
            }
            _this.threads.forEach(function (thread) {
                thread.show = true;
            });
            if (_this.hidden)
                _this.hidden = false;
            if (_this.edge != null) {
                _this.edge.remove();
            }
            _this.addQtip();
            _this.placeHolders.forEach(function (placeHolder) {
                $(placeHolder.ux).show(0);
            });
            g_ViewModel.updatePosition();
        };
        this.scrollIntoView = function () {
            _this.line.td.scrollIntoView();
        };
        this.threadService = {
            changed: function (thread) {
                AJAX.addThread(thread, function (newThread) {
                    newThread = ko.es5.mapping.track(newThread);
                    if (_this.data.id == 0)
                        _this.data.id = newThread.groupId;
                    thread.updateData(newThread);
                    _this.calcHeight();
                    g_ViewModel.updatePosition();
                });
            },
            deleted: function (thread) {
                AJAX.deleteThread(thread, function (newData) {
                    _this.calcHeight();
                    g_ViewModel.updatePosition();
                });
            }
        };
        this.unbind = function () {
            ko.unapplyBindings($(_this.ux), true);
            _this.placeHolders.forEach(function (placeHolder) {
                placeHolder.unbind();
            });
        };
        this.updateData = function (newData) {
            _this.data.status = newData.status;

            // update & add new threads
            newData.threads.forEach(function (newThreadData) {
                var found = _this.findThread(newThreadData.id);
                if (found) {
                    found.updateData(newThreadData);
                } else {
                    _this.threads.push(new CommentThread(newThreadData, _this));
                }
            });

            // get threads no longer in db
            var missing = [];
            _this.threads.forEach(function (thread) {
                var found = _this.findDataThread(newData.threads, thread.data.id);
                if (found == null)
                    missing.push(thread);
            });

            // remove threads no longer in database
            missing.forEach(function (thread) {
                _this.threads.remove(thread);
            });
        };
        this.updatePosition = function () {
            if (_this.line.top() != _this.line.tr.style.top)
                _this.line.top(_this.line.tr.style.top);
        };
        this.visible = ko.es5.mapping.computed(function () {
            if (_this.threads.length == 0 || _this.hidden)
                return false;

            var visible = _this.threads.first(function (thread) {
                return thread.show;
            });
            return (visible != null) ? true : false;
        });
        this.viewModel = viewModel;
        this.data = comment;
        this.init();
        this.status.select.idx = this.data.status;

        if (this.valid) {
            $(this.line.td).addClass("CommentLine");
            this.data.threads.forEach(function (thread) {
                _this.threads.push(new CommentThread(thread, _this));
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
    return ReviewComment;
})();

ko.bindingHandlers.commentBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        viewModel.postBound(element);
    }
};
