/// <reference path="references.ts" />
var PlaceHolderComment = (function () {
    function PlaceHolderComment(comment, table) {
        var _this = this;
        this.line = {
            id: "",
            rowId: "",
            td: HTMLTableCellElement = null,
            tr: HTMLTableRowElement = null,
            table: HTMLTableElement = null
        };
        this.ux = null;
        this.height = ko.es5.mapping.computed(function () {
            return _this.comment.height;
        });
        this.rowColor = ko.es5.mapping.computed(function () {
            var tbody = _this.comment.line.tr.parentElement;
            var className = tbody.className;
            var edgeClass = _this.table.className == "EdgeTable" ? "Edge " : "";
            if (className.indexOf("Unchanged") != -1)
                return edgeClass + "Unchanged";
            if (className.indexOf("Changed") != -1)
                return edgeClass + "Changed";
            if (className.indexOf("Added") != -1)
                return edgeClass + "Added";
            if (className.indexOf("Deleted") != -1)
                return edgeClass + "Deleted";

            return "Unchanged";
        });
        this.bind = function () {
            if (_this.ux != null)
                ko.applyBindings(_this, _this.ux);
        };
        this.createUX = function () {
            _this.ux = UI.createElement("placeHolderTemplate", _this.rowId);
            ko.applyBindings(_this, _this.ux);
        };
        this.hide = function () {
            UI.Effects.hide(_this.ux);
        };
        this.init = function (comment, table) {
            _this.comment = comment;
            _this.table = table;
            _this.id = "PlaceHolder_" + table.id + "-" + _this.comment.lineStamp;
            _this.rowId = "Row-" + _this.id;
            _this.line.tr = table.rows[_this.comment.line.tr.rowIndex];
            _this.line.td = _this.line.tr.cells[1];
            _this.line.rowId = _this.line.tr.id;
            _this.table = table;
        };
        this.move = function (comment, table) {
            _this.remove();
            _this.init(comment, table);
            _this.createUX();
        };
        this.postBound = function (element) {
            //this.ux.style.backgroundColor = "pink";
            $(_this.ux).height(_this.comment.height);
            $(_this.ux).insertAfter(_this.line.tr);

            g_ViewModel.reposition();

            setTimeout(function () {
                $(_this.ux).height(_this.comment.height);
                g_ViewModel.reposition();
            }, 250);
        };
        this.remove = function () {
            if (_this.ux != null) {
                _this.unbind();
                _this.table.deleteRow(_this.ux.rowIndex);
                (_this.ux).remove();
                _this.ux = null;
            }
        };
        this.show = function () {
            UI.Effects.show(_this.ux);
        };
        this.unbind = function () {
            if (_this.ux == null)
                return;
            ko.unapplyBindings($(_this.ux), true);
        };
        this.init(comment, table);

        ko.es5.mapping.track(this);
        this.createUX();
    }
    return PlaceHolderComment;
})();
