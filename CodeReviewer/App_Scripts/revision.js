/// <reference path="references.ts" />
var Revision = (function () {
    function Revision(fileVersion, tabIndex) {
        var _this = this;
        this.bound = false;
        // TODO: Use this to switch fileVersions?
        this.updateData = function () {
        };
        this.postBound = function () {
            _this.bound = true;
            g_ViewModel.postBound();
        };
        this.data = fileVersion;
        this.data.diffHtml = this.data.diffHtml || "";
        this.name = "Revision " + this.data.reviewRevision;
        this.tabIndex = tabIndex;
        this.tabId = "tabs-" + this.tabIndex;
        this.title = this.name;
        this.url = "#" + this.tabId;

        ko.es5.mapping.track(this);
    }
    return Revision;
})();
