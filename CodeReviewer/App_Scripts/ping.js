/// <reference path="references.ts" />
var Ping = (function () {
    function Ping(viewModel) {
        var _this = this;
        this.viewModel = null;
        this.display = false;
        this.click = function () {
            _this.pingButton.disabled = true;
            AJAX.pingChangeListReviewers(_this.viewModel.changeList.data.id, function () {
                alert("pinged, please wait for email to be sent. Thank You");
                _this.pingButton.disabled = false;
            }, function (msg) {
                alert("Failed to ping reviewers. Please contact administrator.  Error: " + msg);
                _this.pingButton.disabled = false;
            });
        };
        this.viewModel = viewModel;
        this.display = this.viewModel.isMe();
        this.display = true;
        this.pingButton = document.getElementById("pingButton");
        ko.es5.mapping.track(this);
    }
    return Ping;
})();

function onPingClick() {
    g_ViewModel.ping.click();
}
