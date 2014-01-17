var ViewModel;
(function (ViewModel) {
    var Settings = (function () {
        function Settings(viewModel) {
            var _this = this;
            this.cookies = {
                name: function () {
                    return "settings";
                },
                load: function () {
                    var cookie = $.cookie(_this.cookies.name());
                    if (cookie != null) {
                        var user = cookie.user;
                        _this.settings.user.hideUnchanged = user.hideUnchanged;
                        _this.settings.user.dualPane = user.dualPane;
                        //var changeList = <Dto.ChangeListSettingsDto> cookie.changeList;
                    }
                },
                remove: function () {
                    $.removeCookie(_this.cookies.name());
                },
                save: function () {
                    var data = {
                        user: _this.settings.user,
                        changeList: _this.settings.changeList
                    };
                    $.cookie(_this.cookies.name(), data, { expires: 365 });
                }
            };
            this.settings = {
                id: g_UserSettings.id,
                key: g_UserSettings.key,
                user: new Dto.UserSettingsDto(),
                changeList: new Dto.ChangeListSettingsDto()
            };
            this.subscriptions = new SubscriptionList(false);
            this.viewModel = null;
            this.apply = function () {
                if (_this.settings.user.hideUnchanged) {
                    $("tbody.Unchanged").hide();
                    $("tbody.Seperator").show();
                } else {
                    $("tbody.Unchanged").show();
                    $("tbody.Seperator").hide();
                }
            };
            this.dispose = function () {
                _this.subscriptions.disposeAll();
            };
            this.initSettings = function () {
                _this.settings.user = JSON.parse(g_UserSettings.value);
                _this.settings.changeList = JSON.parse(g_ChangeListSettings.value);
            };
            this.initSubscriptions = function () {
                _this.subscriptions.addDeferred(_this.settings, function () {
                    _this.cookies.save();
                });

                _this.subscriptions.addDeferred(_this.settings.user, function () {
                    AJAX.updateSettings(0, "settings", _this.settings.user);
                });

                _this.subscriptions.addDeferred(_this.settings.changeList, function (newValue) {
                    AJAX.updateSettings(0, _this.viewModel.changeList.data.CL, _this.settings.changeList);
                });

                _this.subscriptions.add(ko.es5.mapping.getObservable(_this.settings.user, "hideUnchanged"), function (newValue) {
                    _this.apply();
                    _this.viewModel.updatePosition();
                });
            };
            this.onFileChanged = function () {
                _this.apply();
            };
            this.subscribe = function () {
                _this.subscriptions.subscribeAll();
            };
            this.viewModel = viewModel;
            this.cookies.load();
            this.initSettings();
            ko.es5.mapping.track(this);

            this.apply();
            this.initSubscriptions();

            this.viewModel.events.changed.add(function (type) {
                _this.onFileChanged();
                _this.subscribe();
            });

            this.viewModel.events.unloaded.add(function (type) {
                _this.dispose();
            });
        }
        return Settings;
    })();
    ViewModel.Settings = Settings;
})(ViewModel || (ViewModel = {}));
