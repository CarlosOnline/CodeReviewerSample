/// <reference path="references.ts" />

class Revision {
    data: Dto.FileVersionDto;
    bound = false;
    name: string;
    tabId: string;
    tabIndex: number;
    title: string;
    url: string;

    // TODO: Use this to switch fileVersions?
    updateData = () => {
    };

    postBound = () => {
        this.bound = true;
        g_ViewModel.postBound();
    };

    constructor(fileVersion: Dto.FileVersionDto, tabIndex: number) {
        this.data = fileVersion;
        this.data.diffHtml = this.data.diffHtml || "";
        this.name = "Revision " + this.data.reviewRevision;
        this.tabIndex = tabIndex;
        this.tabId = "tabs-" + this.tabIndex;
        this.title = this.name;
        this.url = "#" + this.tabId;

        ko.es5.mapping.track(this);
    }
}
