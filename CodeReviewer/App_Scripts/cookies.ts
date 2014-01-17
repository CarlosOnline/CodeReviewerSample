/// <reference path="references.ts" />

class Cookies {
    name: string = null;
    observables: Array<KnockoutObservable<any>> = [];
    subscriptions = new SubscriptionList(true);

    dispose = () => {
        this.subscriptions.disposeAll();
    };

    load = () => {
        var cookie = $.cookie(this.name);
        if (cookie == null)
            return;

        var idx = 0;
        this.observables.forEach((observable) => {
            var value = cookie[idx++];
            if (value !== undefined)
                observable(value);
        });
    };

    remove = () => {
        $.removeCookie(this.name);
    };

    save = () => {
        var cookie = $.cookie(this.name) || {};

        var idx = 0;
        this.observables.forEach((observable) => {
            cookie[idx++] = observable();
        });
        $.cookie(this.name, cookie, { expires: 365 });
    };

    setName = (name: string) => {
        this.name = name;
    }

        subscribe = () => {
        this.subscriptions.subscribeAll();
    };

    private saveObservable = (newValue, idx: number) => {
        var cookie = $.cookie(this.name) || {};
        cookie[idx] = newValue;
        $.cookie(this.name, cookie, { expires: 365 });
    };

    constructor(name: string, observables: Array<KnockoutObservable<any>>, subscribe = false, deferred = false) {
        this.name = name;
        this.observables = observables;
        this.observables.forEach((observable) => {
            this.subscriptions.add(observable, (newValue) => {
                var idx = this.observables.indexOf(observable);
                this.saveObservable(observable(), idx);
            }, subscribe, deferred);
        });
    }
}
