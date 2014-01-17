/// <reference path="references.ts" />

class PlaceHolderComment {
    comment: ReviewComment;
    line = {
        id: "",
        rowId: "",
        td: HTMLTableCellElement = null,
        tr: HTMLTableRowElement = null,
        table: HTMLTableElement = null,
    };
    id: string;
    rowId: string;
    table: HTMLTableElement;
    ux: HTMLTableRowElement = null;

    height = ko.es5.mapping.computed(() => {
        return this.comment.height;
    });

    rowColor = ko.es5.mapping.computed(() => {
        var tbody = this.comment.line.tr.parentElement;
        var className = tbody.className;
        var edgeClass = this.table.className == "EdgeTable" ? "Edge " : "";
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

    bind = () => {
        if (this.ux != null)
            ko.applyBindings(this, this.ux);
    };

    createUX = () => {
        this.ux = <HTMLTableRowElement> UI.createElement("placeHolderTemplate", this.rowId);
        ko.applyBindings(this, this.ux);
    };

    hide = () => {
        UI.Effects.hide(this.ux);
    };

    init = (comment: ReviewComment, table: HTMLTableElement) => {
        this.comment = comment;
        this.table = table;
        this.id = "PlaceHolder_" + table.id + "-" + this.comment.lineStamp;
        this.rowId = "Row-" + this.id;
        this.line.tr = table.rows[this.comment.line.tr.rowIndex];
        this.line.td = this.line.tr.cells[1];
        this.line.rowId = this.line.tr.id;
        this.table = table;
    };

    move = (comment: ReviewComment, table: HTMLTableElement) => {
        this.remove();
        this.init(comment, table);
        this.createUX();
    }

    postBound = (element: HTMLElement) => {
        //this.ux.style.backgroundColor = "pink";
        $(this.ux).height(this.comment.height);
        $(this.ux).insertAfter(this.line.tr);

        g_ViewModel.reposition();

        setTimeout(() => {
            $(this.ux).height(this.comment.height);
            g_ViewModel.reposition();
        }, 250);
    };

    remove = () => {
        if (this.ux != null) {
            this.unbind();
            this.table.deleteRow(this.ux.rowIndex);
            (<any>this.ux).remove();
            this.ux = null;
        }
    };

    show = () => {
        UI.Effects.show(this.ux);
    };

    unbind = () => {
        if (this.ux == null)
            return;
        ko.unapplyBindings($(this.ux), true);
    };

    constructor(comment: ReviewComment, table: HTMLTableElement) {
        this.init(comment, table);

        ko.es5.mapping.track(this);
        this.createUX();
    }
}
