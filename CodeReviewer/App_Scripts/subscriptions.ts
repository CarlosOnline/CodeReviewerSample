/// <reference path="references.ts" />

class Subscription {
    private dirtyFlag: Knockout.DirtyFlag;
    private subscription: KnockoutSubscription;
    private deferredInterval: number = null;
    private deferredData: any = null;

    constructor(private data: any, private callback: (newValue) => void, subscribe = true, private deferred = false) {
        if (data === null || data === undefined) {
            console.log(data, "null subscription object");
            if (console.trace != undefined)
                console.trace();
            throw Error("Missing subscription object");
        }

        if (this.callback === undefined)
            this.callback = () => { };

        if (this.data === undefined)
            this.data = null;

        if (subscribe) {
            this.subscribe();
        }
    }

    private isMappedObject = (data) => {
        return (data.__ko_mapping__ != undefined || data.__tracked__ != undefined || ko.es5.mapping.isTrackableObject(data));
    };

    public subscribe = (deferred?: boolean) => {
        if (deferred !== undefined)
            this.deferred = deferred;

        if (this.data != null) {
            this.dispose();
            this.dirtyFlag = this.isMappedObject(this.data) ? ko.dirtyFlag(this.data) : null;
        }

        if (this.dirtyFlag != null) {
            this.subscription = this.dirtyFlag.isDirty.subscribe(<any> this.callback);
        } else {
            if (this.data === null)
                return;

            this.subscription = this.data.subscribe((data) => {
                if (!this.deferred)
                    this.callback(data);
                else
                    this.deferredCallback(data, false);
            });
        }
    };

    public dispose = () => {
        if (this.subscription != null) {
            this.subscription.dispose();
            this.subscription = null;
        }
    };

    private deferredCallback = (data: any, Force?: boolean) => {
        this.deferredInterval = this.deferredInterval || null;
        this.deferredData = data;
        data = data || this.deferredData;

        Force = Force || false;
        if (Force) {
            this.deferredData = null;

            if (this.deferredInterval != null) {
                window.clearInterval(this.deferredInterval);
                this.deferredInterval = null;
            }

            this.callback(data);
        }
        else if (this.deferredInterval == null) {
            var subscription = this;
            this.deferredInterval = setInterval(() => {
                subscription.deferredCallback(null, true);
            }, 1000);
        }
        else {
            // save already pending
        }
    };
}

class SubscriptionList {
    private list: Array<Subscription> = [];

    constructor(private subscribe = true) {
    }

    add = (data: any, callback?: (newValue) => void, subscribe?: boolean, deferred?: boolean) => {
        if (data === null || data === undefined) {
            console.log(data, "null subscription object");
            if (console.trace != undefined)
                console.trace();
            throw Error("Missing subscription object");
        }

        if (subscribe === undefined)
            subscribe = this.subscribe;

        var subscription = new Subscription(data, callback, subscribe, deferred);
        this.list.push(subscription);
        return subscription;
    };

    addDeferred = (data: any, callback?: (newValue) => void, subscribe?: boolean) => {
        return this.add(data, callback, subscribe, true);
    };

    remove = (subscription) => {
        var idx = this.list.indexOf(subscription);
        if (idx != -1) {
            this.list.slice(idx, 1);
            return true;
        }
        return false;
    };

    subscribeAll = () => {
        this.list.forEach((subscription) => {
            subscription.subscribe();
        });
    };

    disposeAll = () => {
        this.list.forEach((subscription) => {
            subscription.dispose();
        });
    };
}