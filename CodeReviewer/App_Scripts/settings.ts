module ViewModel {
    export class Settings {
        cookies = {
            name: () => {
                return "settings";
            },
            load: () => {
                var cookie = $.cookie(this.cookies.name());
                if (cookie != null) {
                    var user = <Dto.UserSettingsDto> cookie.user;
                    this.settings.user.hideUnchanged = user.hideUnchanged;
                    this.settings.user.dualPane = user.dualPane;
                    //var changeList = <Dto.ChangeListSettingsDto> cookie.changeList;
                }
            },
            remove: () => {
                $.removeCookie(this.cookies.name());
            },
            save: () => {
                var data = {
                    user: this.settings.user,
                    changeList: this.settings.changeList,
                };
                $.cookie(this.cookies.name(), data, { expires: 365 });
            },
        };

        settings = {
            id: g_UserSettings.id,
            key: g_UserSettings.key,
            user: new Dto.UserSettingsDto(),
            changeList: new Dto.ChangeListSettingsDto(),
        };

        subscriptions = new SubscriptionList(false);

        viewModel: ViewModel.Diff = null;

        apply = () => {
            if (this.settings.user.hideUnchanged) {
                $("tbody.Unchanged").hide();
                $("tbody.Seperator").show();
            } else {
                $("tbody.Unchanged").show();
                $("tbody.Seperator").hide();
            }
        };

        dispose = () => {
            this.subscriptions.disposeAll();
        };

        initSettings = () => {
            this.settings.user = <Dto.UserSettingsDto> JSON.parse(g_UserSettings.value);
            this.settings.changeList = <Dto.ChangeListSettingsDto> JSON.parse(g_ChangeListSettings.value);
        };

        initSubscriptions = () => {
            this.subscriptions.addDeferred(this.settings, () => {
                this.cookies.save();
            });

            this.subscriptions.addDeferred(this.settings.user, () => {
                AJAX.updateSettings(0, "settings", this.settings.user);
            });

            this.subscriptions.addDeferred(this.settings.changeList, (newValue) => {
                AJAX.updateSettings(0, this.viewModel.changeList.data.CL, this.settings.changeList);
            });

            this.subscriptions.add(ko.es5.mapping.getObservable(this.settings.user, "hideUnchanged"), (newValue) => {
                this.apply();
                this.viewModel.updatePosition();
            });
        };

        onFileChanged = () => {
            this.apply();
        };

        subscribe = () => {
            this.subscriptions.subscribeAll();
        };

        constructor(viewModel: ViewModel.Diff) {
            this.viewModel = viewModel;
            this.cookies.load();
            this.initSettings();
            ko.es5.mapping.track(this);

            this.apply();
            this.initSubscriptions();

            this.viewModel.events.changed.add((type: number) => {
                this.onFileChanged();
                this.subscribe();
            });

            this.viewModel.events.unloaded.add((type: number) => {
                this.dispose();
            });
        }
    }
}