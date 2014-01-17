declare function getBaseUrl(): string;
declare function getUserName(): any;
declare function getReviewerAlias(): string;
declare function getDisplayName(): string;
declare function getInitialChangeList(): any;
declare function getInitialChangeListSettings(): any;
declare function getInitialUserSettings(): Dto.UserContextDto;
declare function getChangeListDisplayItems(): Array<Dto.ChangeListDisplayItemDto>;

declare var shortcut: any;

var g_BaseUrl = getBaseUrl();
var g_UserName = getUserName();
var g_ReviewerAlias = getReviewerAlias();
var g_DisplayName = getDisplayName();
var g_ChangeList: Dto.ChangeListDto = null;
var g_ChangeListSettings: Dto.UserContextDto = null;
var g_UserSettings: Dto.UserContextDto = null;
var g_ViewModel: ViewModel.Diff = null;
var g_AllModel: ViewModel.All = null;
var g_SubmitChangeListsModel: ViewModel.SubmitChangeLists = null;

interface JQueryStatic {
    connection: {
        changeListHub: any;
        hub: any;
    };

    contextMenu(p1: any): any;
    getJSON(url: string, data?: any, success?: any, fail?: any): JQueryXHR;
    position(p1: any): any;
    resizable(data: any): any;
    tabs(p1: any, p2?: any): any;
    Color(p1: any): any;
}

interface JQuery {
    accordion(p1: any): any;
    connection(p1: any): any;
    contextMenu(p1: any): any;
    getJSON(url: string, data?: any, success?: any, fail?: any): JQueryXHR;
    position(p1: any): any;
    resizable(data?: any): any;
    tabs(p1: any, p2?: any): any;
}

interface Array<T> {
    first(callbackfn: (value: T, index: number, array: T[]) => boolean, thisArg?: any): T;
    remove(item: T); // for ko.es5.mapping.tracked items
    removeAll(); // for ko.es5.mapping.tracked items
}

module Extensions {
    export module ArrayExt {
        export function first<T>(callback: (value: T, index: number, array: T[]) => boolean, thisArg?: Array<T>) {
            thisArg = thisArg || this;
            var found = thisArg.filter(callback);
            if (found && found.length > 0)
                return found[0];
            return null;
        }

        export function removeAll(thisArg?: Array) {
            thisArg = thisArg || this;
            while (thisArg.length > 0)
                thisArg.pop();
        }

        // TODO: Fix?
        export function remove<T>(elem: T, thisArg?: Array<T>) {
            thisArg = thisArg || this;
            var idx = thisArg.indexOf(elem);
            if (idx >= 0 && idx < thisArg.length)
                thisArg.slice(idx, 1);
        }

        Array.prototype.first = ArrayExt.first;
        //Array.prototype.remove = ArrayExt.remove;
        Array.prototype.removeAll = ArrayExt.removeAll;
    }
}
