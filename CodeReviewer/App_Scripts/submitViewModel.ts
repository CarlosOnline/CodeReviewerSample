/// <reference path="references.ts" />

module ViewModel {
    export class SubmitChangeLists {
        items: Array<ChangeListDisplayItem> = [];
        selected: ChangeListDisplayItem = null;
        subscriptions = new SubscriptionList();

        constructor() {
            var data = getChangeListDisplayItems();
            data.forEach((item) => {
                this.items.push(new ChangeListDisplayItem(item, this, (item: ChangeListDisplayItem) => {
                    this.selected = item;
                }));
            });

            ko.es5.mapping.track(this);

            this.subscriptions.add(ko.getObservable(this, "selected"), (item: ChangeListDisplayItem) => {
                (<HTMLInputElement> $("#CL")[0]).value = item.data.CL;
                (<HTMLInputElement>$("#Description")[0]).value = item.data.description;
            });

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

    export function initSubmitChangeListsViewModel() {
        g_SubmitChangeListsModel = new ViewModel.SubmitChangeLists();
        ko.applyBindings(g_SubmitChangeListsModel);
    }
}