
/// Make table column resizable
module Resizable {

    export class TableColumn {
        cookieName: string = null;
        cookies: Cookies = null;
        callback = function (event: any, ui: any) { };
        colElement: JQuery = null;
        colWidth = 0;
        originalSize = 0;
        td: HTMLTableCellElement = null;
        width: KnockoutObservable<number> = null;

        enable = (td) => {
            if (td.jquery === undefined || !td.jquery)
                td = $(td);

            td.resizable("enable");
        };

        disable = (td) => {
            if (td.jquery === undefined || !td.jquery)
                td = $(td);

            td.resizable("disable").removeClass("ui-state-disabled");
        };

        load = () => {
            if (this.cookies != null)
                this.cookies.load();
            if (this.width() > 0) {
                $(this.td).width(this.width());
            }
        };

        constructor(td: HTMLTableCellElement, callback?: (event, ui) => void, cookieName: string = null) {
            this.td = td;
            this.cookieName = cookieName;
            this.width = ko.observable(0);
            if (cookieName != null) {
                this.cookies = new Cookies(cookieName, [this.width], true, true);
            }
            if (callback || false)
                this.callback = callback;

            $(td).resizable({
                handles: "e",

                // set correct COL element and original size
                start: (event, ui) => {
                    var colIndex = ui.helper.index() + 1;
                    var table = getTable(ui.element[0]);
                    this.colElement = $(table).find("colgroup > col:nth-child(" + colIndex + ")");
                    // get col width (faster than .width() on IE)
                    this.colWidth = parseInt(this.colElement.get(0).style.width, 10);
                    if (isNaN(this.colWidth))
                        this.colWidth = 0;

                    this.originalSize = ui.size.width;
                },

                // set COL width
                resize: (event, ui) => {
                    if (callback != null)
                        callback(event, ui);

                    var resizeDelta = ui.size.width - this.originalSize;
                    var newColWidth = this.colWidth + resizeDelta;
                    this.colElement.width(newColWidth);

                    // height must be set in order to prevent IE9 to set wrong height
                    $(this).css("height", "auto");
                    this.width($(td).width());
                },
            });
        }
    }
}