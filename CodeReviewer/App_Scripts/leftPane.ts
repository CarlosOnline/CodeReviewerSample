/// <reference path="references.ts" />

class LeftPane {
    closed = false;
    subscriptions = new SubscriptionList();
    width = 0;

    cookies: Cookies = null;

    displayContents = ko.es5.mapping.computed<string>(() => {
        return this.closed ? "none" : "block";
    });

    displayLabel = ko.es5.mapping.computed<string>(() => {
        // Label must be displayed block, not inline
        return this.closed ? "block" : "none";
    });

    close = () => {
        // this is pointing to View model, not LeftPane
        // Probably b/c its from the UI
        // this.closed(true);
        if (this.closed !== undefined)
            this.closed = true;
        else
            g_ViewModel.leftPane.closed = true;

        UI.LeftPane.close();
    };

    show = () => {
        // this is pointing to View model, not LeftPane
        // Probably b/c its from the UI
        // this.closed(true);

        if (this.closed !== undefined)
            this.closed = false;
        else
            g_ViewModel.leftPane.closed = false;

        UI.LeftPane.show();
    };

    constructor(private settings: Dto.UserSettingsDto) {
        this.closed = settings.leftPane.closed;
        this.width = settings.leftPane.width;
        if (this.width > 0) {
            UI.LeftPane.width = this.width;
            this.width = UI.LeftPane.width;
        } else {
            this.width = UI.LeftPane.width;
        }

        ko.es5.mapping.track(this);

        this.cookies = new Cookies("leftPane", [
            ko.es5.mapping.getObservable(this, "closed"),
            ko.es5.mapping.getObservable(this, "width")
        ], true, true);

        this.cookies.load();

        UI.LeftPane.Resize.init((width) => {
            if (this.closed)
                return;
            this.width = width || UI.LeftPane.width;
        });

        UI.LeftPane.init(this.width, this.closed);
    }
}
