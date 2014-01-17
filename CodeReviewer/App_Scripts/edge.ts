/// <reference path="references.ts" />

class CommentEdge {
    comment: ReviewComment = null;
    div: HTMLElement = null;
    divComment: HTMLElement = null;
    edge: CommentEdge = null;
    id = "";
    ux: HTMLElement = null;

    top = 0;
    left = 0;

    display = {
        edge: ko.es5.mapping.computed(() => {
            return this.comment.hidden ? "inline" : "none";
        }),
    };

    classes = ko.es5.mapping.computed(() => {
        return this.comment.status.icon.classes;
    });

    click = () => {
        this.comment.show();
        //$(this.ux).contextMenu();
    };

    color = ko.es5.mapping.computed(() => {
        return this.comment.status.color;
    });

    showContextMenu = () => {
        // TODO: var menu =
    };

    remove = () => {
        if (this.ux != null) {
            $(this.ux).remove();
            this.ux = null;
        }
    };

    reposition = () => {
        if (this.ux == null)
            return;

        var top = $(this.comment.line.td).offset().top - $(this.comment.line.table).offset().top - $(this.divComment).scrollTop();
        var position = {
            of: $(this.div),
            position: 'relative',
            top: top - 16 + 'px',
            "z-index": 9999,
            offset: '0 0'
        };
        $(this.ux).css(position);
    };

    show = () => {
        if (this.ux != null) {
            return;
        }
        this.div = UI.Revisions.Current.EdgeTable;
        this.divComment = UI.Revisions.Current.RightTable;

        // dynamic creation to allow proper binding
        // otherwise knockout wont bind it properly
        this.ux = UI.createElement("commentEdgeTemplate", this.id, this.div[0].firstElementChild);
        if (this.ux == null)
            return;

        this.reposition();

        ko.applyBindings(this, this.ux);
    };

    postBound = (element: HTMLElement) => {
        g_ViewModel.updatePosition();
    };

    constructor(comment: ReviewComment) {
        this.edge = this; // for template binding
        this.comment = comment;
        this.id = "Edge-" + this.comment.id;

        ko.es5.mapping.track(this);
        this.show();
    }
}
