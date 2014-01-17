/// <reference path="references.ts" />
var Stage = (function () {
    function Stage(viewModel) {
        var _this = this;
        this.viewModel = viewModel;
        this.subscriptions = new SubscriptionList();
        this.id = ko.es5.mapping.computed(function () {
            return _this.resource.id;
        });
        this.idx = ko.es5.mapping.computed(function () {
            return _this.viewModel.changeList.data.stage;
        });
        this.resource = ko.es5.mapping.computed(function () {
            return Resource.Stage.getResource(_this.viewModel.changeList.data.stage);
        });
        this.value = ko.es5.mapping.computed(function () {
            return _this.resource.value;
        });
        this.title = ko.es5.mapping.computed(function () {
            return _this.value;
        });
        this.key = ko.es5.mapping.computed(function () {
            return _this.resource.key;
        });
        this.click = function () {
        };
        this.select = null;
        this.icon = {
            title: ko.es5.mapping.computed(function () {
                return _this.resource.icon.title;
            }),
            classes: ko.es5.mapping.computed(function () {
                return _this.resource.icon.classes;
            }),
            click: function () {
            }
        };
        this.updateData = function (newData) {
            _this.subscriptions.disposeAll();
            _this.viewModel.changeList.data.stage = newData.status;
            _this.subscriptions.subscribeAll();
        };
        ko.es5.mapping.track(this);
        this.select = new UI.select(this.value, Resource.Review.getResource().Status.list, function () {
            return true;
            //return this.viewModel.isMe() ? true : false;
        }, function (newValue) {
            AJAX.setChangeListStatus(newValue, function (newData) {
                _this.updateData(newData);
            });
        });
        this.select.subscribe();

        this.subscriptions.add(ko.es5.mapping.getObservable(this, "resource"), function (newValue) {
            if (newValue.id != _this.viewModel.changeList.data.stage) {
                _this.viewModel.changeList.data.stage = newValue.id;
            }
        });
    }
    return Stage;
})();
