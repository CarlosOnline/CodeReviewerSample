/// <reference path="references.ts" />
var g_TestMode = true;

var Reviewer;
(function (Reviewer) {
    var Status = (function () {
        function Status(model, callback) {
            var _this = this;
            this.model = model;
            this.idx = ko.es5.mapping.computed(function () {
                return _this.model.data.status;
            });
            this.key = ko.es5.mapping.computed(function () {
                return _this.model.resource.key;
            });
            this.value = ko.es5.mapping.computed(function () {
                return _this.model.resource.value;
            });
            this.title = ko.es5.mapping.computed(function () {
                return _this.value;
            });
            this.select = new UI.select(this.model.selectValue, [
                Resources.Reviewer.Status.Looking.value,
                Resources.Reviewer.Status.SignedOff.value,
                Resources.Reviewer.Status.WaitingOnAuthor.value
            ], ko.computed(function () {
                return !_this.model.isMe || _this.model.isReviewer ? false : true;
            }), callback);

            ko.es5.mapping.track(this);
        }
        return Status;
    })();
    Reviewer.Status = Status;

    var Person = (function () {
        function Person(data) {
            var _this = this;
            this.subscriptions = new SubscriptionList();
            this.deleteReviewer = {
                display: ko.es5.mapping.computed(function () {
                    return _this.isReviewer ? "none" : "";
                }),
                click: function () {
                    if (_this.isReviewer)
                        return;

                    AJAX.deleteReviewer(_this.data.id);
                    _this.remove();
                }
            };
            this.display = ko.es5.mapping.computed(function () {
                return true;
                //return this.isReviewer() ? false : true;
            });
            this.resource = ko.es5.mapping.computed(function () {
                switch (_this.data.status) {
                    case Resources.Reviewer.Status.NotLookedAtYet.id:
                        return Resources.Reviewer.Status.NotLookedAtYet;
                    case Resources.Reviewer.Status.Looking.id:
                        return Resources.Reviewer.Status.Looking;
                    case Resources.Reviewer.Status.SignedOff.id:
                        return Resources.Reviewer.Status.SignedOff;
                    case Resources.Reviewer.Status.SignedOffWithComments.id:
                        return Resources.Reviewer.Status.SignedOffWithComments;
                    case Resources.Reviewer.Status.WaitingOnAuthor.id:
                        return Resources.Reviewer.Status.WaitingOnAuthor;
                    case Resources.Reviewer.Status.Complete.id:
                        return Resources.Reviewer.Status.Complete;
                    default:
                        return Resources.Reviewer.Status.None;
                }
            });
            this.resourceFromString = function (value) {
                switch (value) {
                    case Resources.Reviewer.Status.NotLookedAtYet.value:
                        return Resources.Reviewer.Status.NotLookedAtYet;
                    case Resources.Reviewer.Status.Looking.value:
                        return Resources.Reviewer.Status.Looking;
                    case Resources.Reviewer.Status.SignedOff.value:
                        return Resources.Reviewer.Status.SignedOff;
                    case Resources.Reviewer.Status.SignedOffWithComments.value:
                        return Resources.Reviewer.Status.SignedOffWithComments;
                    case Resources.Reviewer.Status.WaitingOnAuthor.value:
                        return Resources.Reviewer.Status.WaitingOnAuthor;
                    case Resources.Reviewer.Status.Complete.value:
                        return Resources.Reviewer.Status.Complete;
                    default:
                        return Resources.Reviewer.Status.None;
                }
            };
            this.icon = {
                classes: ko.es5.mapping.computed(function () {
                    return _this.resource.icon.classes;
                }),
                click: function () {
                },
                title: ko.es5.mapping.computed(function () {
                    return _this.status.title;
                })
            };
            this.id = ko.es5.mapping.computed(function () {
                return "Reviewer-" + _this.data.reviewerAlias.replace(".", "-");
            });
            this.isMe = ko.es5.mapping.computed(function () {
                if (g_TestMode) {
                    if (_this.data.reviewerAlias == "Test.Me2")
                        return true;
                }

                return _this.data.reviewerAlias == g_ReviewerAlias ? true : false;
            });
            this.isReviewer = ko.es5.mapping.computed(function () {
                return _this.data.reviewerAlias == g_UserName || _this.data.reviewerAlias == g_ReviewerAlias ? true : false;
            });
            this.name = {
                classes: ko.es5.mapping.computed(function () {
                    return _this.status.key;
                }),
                click: function () {
                },
                title: ko.es5.mapping.computed(function () {
                    return _this.data.reviewerAlias;
                }),
                value: ko.es5.mapping.computed(function () {
                    return _this.data.reviewerAlias;
                })
            };
            this.onStatusChange = function (newValue) {
                if (newValue != _this.data.status) {
                    _this.data.status = _this.resourceFromString(newValue).id;
                }
            };
            this.selectValue = ko.es5.mapping.computed(function () {
                switch (_this.data.status) {
                    default:
                    case Resources.Reviewer.Status.NotLookedAtYet.id:
                    case Resources.Reviewer.Status.Looking.id:
                        return Resources.Reviewer.Status.Looking.value;
                    case Resources.Reviewer.Status.SignedOff.id:
                    case Resources.Reviewer.Status.SignedOffWithComments.id:
                    case Resources.Reviewer.Status.Complete.id:
                        return Resources.Reviewer.Status.SignedOff.value;
                    case Resources.Reviewer.Status.WaitingOnAuthor.id:
                        return Resources.Reviewer.Status.WaitingOnAuthor.value;
                }
            });
            this.updateData = function (newData) {
                _this.subscriptions.disposeAll();
                _this.status.select.dispose();

                _this.data.id = newData.id;
                _this.data.reviewerAlias = newData.reviewerAlias;
                _this.data.changeListId = newData.changeListId;
                _this.data.status = newData.status;
                _this.status.select.value = _this.resource.value;

                _this.subscriptions.subscribeAll();
                _this.status.select.subscribe();
            };
            this.load = function () {
                if (_this.isMe) {
                    if (_this.status.select.value < _this.status.select.list[0])
                        _this.status.select.value = _this.status.select.list[0];
                }
            };
            this.postBound = function (element) {
                _this.subscriptions.subscribeAll();
            };
            this.remove = function () {
                g_ViewModel.reviewers.remove(_this);
            };
            this.data = data;
            this.status = new Status(this, this.onStatusChange);

            ko.es5.mapping.track(this);

            this.subscriptions.add(ko.es5.mapping.getObservable(this.data, "status"), function (newDataData) {
                AJAX.addReviewer(_this.data, function (newData) {
                    this.updateData(newData);
                });
            }, false);

            this.load();
            this.subscriptions.subscribeAll();
            this.status.select.subscribe();
        }
        return Person;
    })();
    Reviewer.Person = Person;

    var Service = (function () {
        function Service(viewModel) {
            var _this = this;
            this.viewModel = viewModel;
            this.add = {
                display: "none",
                icon: {
                    display: ""
                }
            };
            this.data = {
                reviewerAlias: "",
                changeListId: 0
            };
            this.ux = {
                add: document.getElementById("addDisplay")
            };
            this.addReviewer = function () {
                if (_this.data.reviewerAlias == "") {
                    console.log("empty reviewer - cannot add");
                    return;
                }

                var input = new Dto.ReviewerDto();
                input.id = 0;
                input.reviewerAlias = _this.data.reviewerAlias;
                input.changeListId = _this.data.changeListId;
                input.status = 0;
                input.requestType = 0;

                AJAX.addReviewer(input, function (newData) {
                    _this.updateReviewer(newData);
                });
            };
            this.updateReviewer = function (data) {
                var found = _this.viewModel.reviewers.first(function (reviewer) {
                    return reviewer.data.id == data.id || reviewer.data.id == data.id || reviewer.data.reviewerAlias == data.reviewerAlias || reviewer.data.reviewerAlias == data.reviewerAlias;
                });

                if (found != null) {
                    found.updateData(data);
                    UI.Effects.flash("#" + found.id);
                } else {
                    _this.viewModel.reviewers.push(new Person(data));
                }
            };
            this.removeReviewer = function (id) {
                var found = _this.viewModel.reviewers.first(function (reviewer) {
                    return reviewer.data.id == id;
                });

                if (found != null) {
                    found.remove();
                }
            };
            this.data.changeListId = this.viewModel.changeList.data.id;
            this.viewModel = viewModel;
            $.extend(true, this.add, {
                icon: {
                    click: function () {
                        _this.data.reviewerAlias = "";
                        _this.add.display = "inline";
                        _this.add.icon.display = "none";
                        document.getElementById("addDisplay").focus();
                    }
                },
                submit: {
                    click: function () {
                        _this.add.display = "none";
                        _this.add.icon.display = "";
                        _this.addReviewer();
                    },
                    enabled: ko.es5.mapping.computed(function () {
                        return _this.data.reviewerAlias != "" ? true : false;
                    })
                },
                cancel: {
                    click: function () {
                        _this.data.reviewerAlias = "";
                        _this.add.display = "none";
                        _this.add.icon.display = "";
                    }
                }
            });
            ko.es5.mapping.track(this);
        }
        return Service;
    })();
    Reviewer.Service = Service;
})(Reviewer || (Reviewer = {}));

ko.bindingHandlers.reviewerBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // console.log("postBound", element);
        //viewModel.postBound(element);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // console.log("postBound", element);
        viewModel.postBound(element);
    }
};
