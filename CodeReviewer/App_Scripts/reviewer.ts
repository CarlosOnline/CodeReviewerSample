/// <reference path="references.ts" />

var g_TestMode = true;

module Reviewer {
    export class Status {
        select: UI.select;

        click: () => {
        }

        idx = ko.es5.mapping.computed<number>(() => {
            return this.model.data.status;
        });

        key = ko.es5.mapping.computed<string>(() => {
            return this.model.resource.key;
        });

        value = ko.es5.mapping.computed<string>(() => {
            return this.model.resource.value;
        });

        title = ko.es5.mapping.computed<string>(() => {
            return this.value;
        });

        constructor(private model: Reviewer.Person, callback?: (newValue) => void) {
            this.select = new UI.select(this.model.selectValue,
                [
                    Resources.Reviewer.Status.Looking.value,
                    Resources.Reviewer.Status.SignedOff.value,
                    Resources.Reviewer.Status.WaitingOnAuthor.value
                ],
                ko.computed(() => { // TODO: es5?
                    return !this.model.isMe || this.model.isReviewer ? false : true;
                }),
                callback);

            ko.es5.mapping.track(this);
        }
    } // class Status

    export class Person {
        data: Dto.ReviewerDto;
        status: Status;
        subscriptions = new SubscriptionList();

        deleteReviewer = {
            display: ko.es5.mapping.computed<string>(() => {
                return this.isReviewer ? "none" : ""; // false : true;
            }),

            click: () => {
                if (this.isReviewer)
                    return;

                AJAX.deleteReviewer(this.data.id);
                this.remove();
            },
        };

        display = ko.es5.mapping.computed<boolean>(() => {
            return true;
            //return this.isReviewer() ? false : true;
        });

        resource = ko.es5.mapping.computed<Resource.Types.Reviewer.Status>(() => {
            switch (this.data.status) {
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

        resourceFromString = (value: string) => {
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

        icon = {
            classes: ko.es5.mapping.computed<string>(() => {
                return this.resource.icon.classes;
            }),

            click: () => {
            },

            title: ko.es5.mapping.computed<string>(() => {
                return this.status.title;
            }),
        };

        id = ko.es5.mapping.computed<string>(() => {
            return "Reviewer-" + this.data.reviewerAlias.replace(".", "-");
        });

        isMe = ko.es5.mapping.computed<boolean>(() => {
            if (g_TestMode) {
                if (this.data.reviewerAlias == "Test.Me2")
                    return true;
            }

            return this.data.reviewerAlias == g_ReviewerAlias ? true : false;
        });

        isReviewer = ko.es5.mapping.computed<boolean>(() => {
            return this.data.reviewerAlias == g_UserName ||
                this.data.reviewerAlias == g_ReviewerAlias ? true : false;
        });

        name = {
            classes: ko.es5.mapping.computed<string>(() => {
                return this.status.key;
            }),

            click: () => {
            },

            title: ko.es5.mapping.computed<string>(() => {
                return this.data.reviewerAlias;
            }),

            value: ko.es5.mapping.computed<string>(() => {
                return this.data.reviewerAlias;
            }),
        };

        onStatusChange = (newValue) => {
            if (newValue != this.data.status) {
                this.data.status = this.resourceFromString(newValue).id;
            }
        };

        selectValue = ko.es5.mapping.computed<string>(() => {
            switch (this.data.status) {
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

        updateData = (newData: Dto.ReviewerDto) => {
            this.subscriptions.disposeAll();
            this.status.select.dispose();

            this.data.id = newData.id;
            this.data.reviewerAlias = newData.reviewerAlias;
            this.data.changeListId = newData.changeListId;
            this.data.status = newData.status;
            this.status.select.value = this.resource.value;

            this.subscriptions.subscribeAll();
            this.status.select.subscribe();
        };

        load = () => {
            if (this.isMe) {
                if (this.status.select.value < this.status.select.list[0])
                    this.status.select.value = this.status.select.list[0];
            }
        };

        postBound = (element) => {
            this.subscriptions.subscribeAll();
        };

        remove = () => {
            g_ViewModel.reviewers.remove(this);
        };

        constructor(data: Dto.ReviewerDto) {
            this.data = data;
            this.status = new Status(this, this.onStatusChange);

            ko.es5.mapping.track(this);

            this.subscriptions.add(ko.es5.mapping.getObservable(this.data, "status"), (newDataData) => {
                AJAX.addReviewer(this.data,
                    function (newData: Dto.ReviewerDto) {
                        this.updateData(newData);
                    });
            }, false);

            this.load();
            this.subscriptions.subscribeAll();
            this.status.select.subscribe();
        }
    } // class Person

    export class Service {
        add = {
            display: "none",
            icon: {
                display: "",
            },
        };
        data = {
            reviewerAlias: "",
            changeListId: 0,
        };
        ux = {
            add: document.getElementById("addDisplay"),
        };

        constructor(private viewModel: ViewModel.Diff) {
            this.data.changeListId = this.viewModel.changeList.data.id;
            this.viewModel = viewModel;
            $.extend(true, this.add, {
                icon: {
                    click: () => {
                        this.data.reviewerAlias = "";
                        this.add.display = "inline";
                        this.add.icon.display = "none";
                        document.getElementById("addDisplay").focus();
                    },
                },

                submit: {
                    click: () => {
                        this.add.display = "none";
                        this.add.icon.display = "";
                        this.addReviewer();
                    },

                    enabled: ko.es5.mapping.computed<boolean>(() => {
                        return this.data.reviewerAlias != "" ? true : false;
                    }),
                },

                cancel: {
                    click: () => {
                        this.data.reviewerAlias = "";
                        this.add.display = "none";
                        this.add.icon.display = "";
                    },
                },
            });
            ko.es5.mapping.track(this);
        }

        addReviewer = () => {
            if (this.data.reviewerAlias == "") {
                console.log("empty reviewer - cannot add");
                return;
            }

            var input = new Dto.ReviewerDto();
            input.id = 0;
            input.reviewerAlias = this.data.reviewerAlias;
            input.changeListId = this.data.changeListId;
            input.status = 0;
            input.requestType = 0;

            AJAX.addReviewer(input, (newData) => {
                this.updateReviewer(newData);
            });
        };

        updateReviewer = (data: Dto.ReviewerDto) => {
            var found = this.viewModel.reviewers.first((reviewer) => {
                return reviewer.data.id == data.id || reviewer.data.id == data.id || reviewer.data.reviewerAlias == data.reviewerAlias || reviewer.data.reviewerAlias == data.reviewerAlias;
            });

            if (found != null) {
                found.updateData(data);
                UI.Effects.flash("#" + found.id);
            } else {
                this.viewModel.reviewers.push(new Person(data));
            }
        };

        removeReviewer = (id) => {
            var found = this.viewModel.reviewers.first((reviewer) => {
                return reviewer.data.id == id;
            });

            if (found != null) {
                found.remove();
            }
        };
    } // class Service
} // module Reviewer

interface KnockoutBindingHandlers {
    reviewerBind: KnockoutBindingHandler;
}

ko.bindingHandlers.reviewerBind = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // console.log("postBound", element);
        //viewModel.postBound(element);
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // console.log("postBound", element);
        viewModel.postBound(element);
    }
}
