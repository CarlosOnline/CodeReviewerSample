/// <reference path="references.ts" />
var ChangeListDisplayItem = (function () {
    function ChangeListDisplayItem(data, viewModel, clickHandler) {
        if (typeof clickHandler === "undefined") { clickHandler = null; }
        var _this = this;
        this.clickHandler = null;
        this.data = null;
        this.viewModel = null;
        this.title = ko.es5.mapping.computed(function () {
            return "Review " + _this.data.CL;
        });
        this.url = ko.es5.mapping.computed(function () {
            return "/Review/" + _this.data.CL;
        });
        this.click = function () {
            if (_this.clickHandler != null) {
                _this.clickHandler(_this);
                return;
            }
            window.location.href = window.location.href + _this.url;
        };
        this.selected = function (context, eventObj) {
            var td = getTD(eventObj.target);
            if (td.cellIndex == 0)
                _this.click();
else
                _this.viewModel.selected = _this;
        };
        this.clickHandler = clickHandler;
        this.data = data;
        this.viewModel = viewModel;

        ko.es5.mapping.track(this);
    }
    return ChangeListDisplayItem;
})();
