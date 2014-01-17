/// <reference path="references.ts" />
var Cookies = (function () {
    function Cookies(name, observables, subscribe, deferred) {
        if (typeof subscribe === "undefined") { subscribe = false; }
        if (typeof deferred === "undefined") { deferred = false; }
        var _this = this;
        this.name = null;
        this.observables = [];
        this.subscriptions = new SubscriptionList(true);
        this.dispose = function () {
            _this.subscriptions.disposeAll();
        };
        this.load = function () {
            var cookie = $.cookie(_this.name);
            if (cookie == null)
                return;

            var idx = 0;
            _this.observables.forEach(function (observable) {
                var value = cookie[idx++];
                if (value !== undefined)
                    observable(value);
            });
        };
        this.remove = function () {
            $.removeCookie(_this.name);
        };
        this.save = function () {
            var cookie = $.cookie(_this.name) || {};

            var idx = 0;
            _this.observables.forEach(function (observable) {
                cookie[idx++] = observable();
            });
            $.cookie(_this.name, cookie, { expires: 365 });
        };
        this.setName = function (name) {
            _this.name = name;
        };
        this.subscribe = function () {
            _this.subscriptions.subscribeAll();
        };
        this.saveObservable = function (newValue, idx) {
            var cookie = $.cookie(_this.name) || {};
            cookie[idx] = newValue;
            $.cookie(_this.name, cookie, { expires: 365 });
        };
        this.name = name;
        this.observables = observables;
        this.observables.forEach(function (observable) {
            _this.subscriptions.add(observable, function (newValue) {
                var idx = _this.observables.indexOf(observable);
                _this.saveObservable(observable(), idx);
            }, subscribe, deferred);
        });
    }
    return Cookies;
})();
