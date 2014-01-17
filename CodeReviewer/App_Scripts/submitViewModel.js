/// <reference path="references.ts" />
var ViewModel;
(function (ViewModel) {
    var SubmitChangeLists = (function () {
        function SubmitChangeLists() {
            var _this = this;
            this.items = [];
            this.selected = null;
            this.subscriptions = new SubscriptionList();
            var data = getChangeListDisplayItems();
            data.forEach(function (item) {
                _this.items.push(new ChangeListDisplayItem(item, _this, function (item) {
                    _this.selected = item;
                }));
            });

            ko.es5.mapping.track(this);

            this.subscriptions.add(ko.getObservable(this, "selected"), function (item) {
                ($("#CL")[0]).value = item.data.CL;
                ($("#Description")[0]).value = item.data.description;
            });

            if (this.items.length > 0) {
                this.selected = this.items[0];
                $(".items").show();
            } else {
                $(".items").hide();
                $(".noReviews").show();
            }
        }
        return SubmitChangeLists;
    })();
    ViewModel.SubmitChangeLists = SubmitChangeLists;

    function initSubmitChangeListsViewModel() {
        g_SubmitChangeListsModel = new ViewModel.SubmitChangeLists();
        ko.applyBindings(g_SubmitChangeListsModel);
    }
    ViewModel.initSubmitChangeListsViewModel = initSubmitChangeListsViewModel;
})(ViewModel || (ViewModel = {}));
