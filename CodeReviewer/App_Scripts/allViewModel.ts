/// <reference path="references.ts" />

module ViewModel {
    export class All {
        items: Array<ChangeListDisplayItem> = [];
        selected: ChangeListDisplayItem = null;
        subscriptions = new SubscriptionList();

        constructor() {
            var data = getChangeListDisplayItems();
            data.forEach((item) => {
                this.items.push(new ChangeListDisplayItem(item, this));
            });

            ko.es5.mapping.track(this);
            if (this.items.length > 0) {
                this.selected = this.items[0];
                $(".items").show();
            }
            else {
                $(".items").hide();
                $(".noReviews").show();
            }
        }
    }

    export function initAllViewModel() {
        g_AllModel = new ViewModel.All();
        ko.applyBindings(g_AllModel);
    }
}