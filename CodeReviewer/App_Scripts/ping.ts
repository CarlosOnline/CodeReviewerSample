/// <reference path="references.ts" />

class Ping {
    pingButton: HTMLElement;
    viewModel: ViewModel.Diff = null;
    display = false;

    click = () => {
        this.pingButton.disabled = true;
        AJAX.pingChangeListReviewers(this.viewModel.changeList.data.id, () => {
            alert("pinged, please wait for email to be sent. Thank You");
            this.pingButton.disabled = false;
        },
        (msg) => {
            alert("Failed to ping reviewers. Please contact administrator.  Error: " + msg);
            this.pingButton.disabled = false;
        });
    };

    constructor(viewModel: ViewModel.Diff) {
        this.viewModel = viewModel;
        this.display = this.viewModel.isMe();
        this.display = true; // TODO: Remove
        this.pingButton = document.getElementById("pingButton");
        ko.es5.mapping.track(this);
    }
}

function onPingClick() {
    g_ViewModel.ping.click();
}