/// <reference path="references.ts" />

class ChangeListDisplayItem {

    clickHandler: (item: ChangeListDisplayItem) => any = null;
    data: Dto.ChangeListDisplayItemDto = null;
    viewModel: ViewModel.All = null;

    title = ko.es5.mapping.computed(() => {
        return "Review " + this.data.CL;
    });

    url = ko.es5.mapping.computed(() => {
        return "/Review/" + this.data.CL;
    });

    click = () => {
        if (this.clickHandler != null) {
            this.clickHandler(this);
            return;
        }
        window.location.href = window.location.href + this.url;
    };

    selected = (context, eventObj: JQueryEventObject) => {
        var td = getTD(<HTMLElement> eventObj.target);
        if (td.cellIndex == 0)
            this.click();
        else
            this.viewModel.selected = this;
    };

    constructor(data: Dto.ChangeListDisplayItemDto, viewModel: ViewModel.All, clickHandler: (item: ChangeListDisplayItem) => any = null) {
        this.clickHandler = clickHandler;
        this.data = data;
        this.viewModel = viewModel;

        ko.es5.mapping.track(this);
    }
}