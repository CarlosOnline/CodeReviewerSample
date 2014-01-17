/// <reference path="references.ts" />
var ViewModel;
(function (ViewModel) {
    var All = (function () {
        function All() {
            var _this = this;
            this.items = [];
            this.selected = null;
            this.subscriptions = new SubscriptionList();
            var data = getChangeListDisplayItems();
            data.forEach(function (item) {
                _this.items.push(new ChangeListDisplayItem(item, _this));
            });

            ko.es5.mapping.track(this);
            if (this.items.length > 0) {
                this.selected = this.items[0];
                $(".items").show();
            } else {
                $(".items").hide();
                $(".noReviews").show();
            }
        }
        return All;
    })();
    ViewModel.All = All;

    function initAllViewModel() {
        g_AllModel = new ViewModel.All();
        ko.applyBindings(g_AllModel);
    }
    ViewModel.initAllViewModel = initAllViewModel;
})(ViewModel || (ViewModel = {}));
