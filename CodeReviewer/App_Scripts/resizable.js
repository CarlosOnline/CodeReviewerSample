/// Make table column resizable
var Resizable;
(function (Resizable) {
    var TableColumn = (function () {
        function TableColumn(td, callback, cookieName) {
            if (typeof cookieName === "undefined") { cookieName = null; }
            var _this = this;
            this.cookieName = null;
            this.cookies = null;
            this.callback = function (event, ui) {
            };
            this.colElement = null;
            this.colWidth = 0;
            this.originalSize = 0;
            this.td = null;
            this.width = null;
            this.enable = function (td) {
                if (td.jquery === undefined || !td.jquery)
                    td = $(td);

                td.resizable("enable");
            };
            this.disable = function (td) {
                if (td.jquery === undefined || !td.jquery)
                    td = $(td);

                td.resizable("disable").removeClass("ui-state-disabled");
            };
            this.load = function () {
                if (_this.cookies != null)
                    _this.cookies.load();
                if (_this.width() > 0) {
                    $(_this.td).width(_this.width());
                }
            };
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
                start: function (event, ui) {
                    var colIndex = ui.helper.index() + 1;
                    var table = getTable(ui.element[0]);
                    _this.colElement = $(table).find("colgroup > col:nth-child(" + colIndex + ")");

                    // get col width (faster than .width() on IE)
                    _this.colWidth = parseInt(_this.colElement.get(0).style.width, 10);
                    if (isNaN(_this.colWidth))
                        _this.colWidth = 0;

                    _this.originalSize = ui.size.width;
                },
                // set COL width
                resize: function (event, ui) {
                    if (callback != null)
                        callback(event, ui);

                    var resizeDelta = ui.size.width - _this.originalSize;
                    var newColWidth = _this.colWidth + resizeDelta;
                    _this.colElement.width(newColWidth);

                    // height must be set in order to prevent IE9 to set wrong height
                    $(_this).css("height", "auto");
                    _this.width($(td).width());
                }
            });
        }
        return TableColumn;
    })();
    Resizable.TableColumn = TableColumn;
})(Resizable || (Resizable = {}));
