/// <reference path="references.ts" />

class Stage {
    subscriptions = new SubscriptionList();

    id = ko.es5.mapping.computed<number>(() => {
        return this.resource.id;
    });

    idx = ko.es5.mapping.computed<number>(() => {
        return this.viewModel.changeList.data.stage;
    });

    resource = ko.es5.mapping.computed<Resource.Types.Stage>(() => {
        return Resource.Stage.getResource(this.viewModel.changeList.data.stage);
    });

    value = ko.es5.mapping.computed<string>(() => {
        return this.resource.value;
    });

    title = ko.es5.mapping.computed<string>(() => {
        return this.value;
    });

    key = ko.es5.mapping.computed<string>(() => {
        return this.resource.key;
    });

    click = () => {
    };

    select = null;

    icon = {
        title: ko.es5.mapping.computed<string>(() => {
            return this.resource.icon.title;
        }),

        classes: ko.es5.mapping.computed<string>(() => {
            return this.resource.icon.classes;
        }),

        click: () => {
        },
    };
    updateData = (newData: Dto.ChangeListStatusDto) => {
        this.subscriptions.disposeAll();
        this.viewModel.changeList.data.stage = newData.status;
        this.subscriptions.subscribeAll();
    };

    constructor(private viewModel: ViewModel.Diff) {
        ko.es5.mapping.track(this);
        this.select = new UI.select(this.value,
            Resource.Review.getResource().Status.list,
            () => {
                return true;
                //return this.viewModel.isMe() ? true : false;
            },
            (newValue: string) => {
                AJAX.setChangeListStatus(newValue, (newData) => {
                    this.updateData(newData);
                });
            });
        this.select.subscribe();

        this.subscriptions.add(ko.es5.mapping.getObservable(this, "resource"), (newValue: Resource.Types.Stage) => {
            if (newValue.id != this.viewModel.changeList.data.stage) {
                this.viewModel.changeList.data.stage = newValue.id;
            }
        });
    }
}
