/// <reference path="references.ts" />
var LeftPane = (function () {
    function LeftPane(settings) {
        var _this = this;
        this.settings = settings;
        this.closed = false;
        this.subscriptions = new SubscriptionList();
        this.width = 0;
        this.cookies = null;
        this.displayContents = ko.es5.mapping.computed(function () {
            return _this.closed ? "none" : "block";
        });
        this.displayLabel = ko.es5.mapping.computed(function () {
            // Label must be displayed block, not inline
            return _this.closed ? "block" : "none";
        });
        this.close = function () {
            if (_this.closed !== undefined)
                _this.closed = true;
else
                g_ViewModel.leftPane.closed = true;

            UI.LeftPane.close();
        };
        this.show = function () {
            if (_this.closed !== undefined)
                _this.closed = false;
else
                g_ViewModel.leftPane.closed = false;

            UI.LeftPane.show();
        };
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

        UI.LeftPane.Resize.init(function (width) {
            if (_this.closed)
                return;
            _this.width = width || UI.LeftPane.width;
        });

        UI.LeftPane.init(this.width, this.closed);
    }
    return LeftPane;
})();
