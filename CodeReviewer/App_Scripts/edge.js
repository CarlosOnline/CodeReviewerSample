/// <reference path="references.ts" />
var CommentEdge = (function () {
    function CommentEdge(comment) {
        var _this = this;
        this.comment = null;
        this.div = null;
        this.divComment = null;
        this.edge = null;
        this.id = "";
        this.ux = null;
        this.top = 0;
        this.left = 0;
        this.display = {
            edge: ko.es5.mapping.computed(function () {
                return _this.comment.hidden ? "inline" : "none";
            })
        };
        this.classes = ko.es5.mapping.computed(function () {
            return _this.comment.status.icon.classes;
        });
        this.click = function () {
            _this.comment.show();
            //$(this.ux).contextMenu();
        };
        this.color = ko.es5.mapping.computed(function () {
            return _this.comment.status.color;
        });
        this.showContextMenu = function () {
            // TODO: var menu =
        };
        this.remove = function () {
            if (_this.ux != null) {
                $(_this.ux).remove();
                _this.ux = null;
            }
        };
        this.reposition = function () {
            if (_this.ux == null)
                return;

            var top = $(_this.comment.line.td).offset().top - $(_this.comment.line.table).offset().top - $(_this.divComment).scrollTop();
            var position = {
                of: $(_this.div),
                position: 'relative',
                top: top - 16 + 'px',
                "z-index": 9999,
                offset: '0 0'
            };
            $(_this.ux).css(position);
        };
        this.show = function () {
            if (_this.ux != null) {
                return;
            }
            _this.div = UI.Revisions.Current.EdgeTable;
            _this.divComment = UI.Revisions.Current.RightTable;

            // dynamic creation to allow proper binding
            // otherwise knockout wont bind it properly
            _this.ux = UI.createElement("commentEdgeTemplate", _this.id, _this.div[0].firstElementChild);
            if (_this.ux == null)
                return;

            _this.reposition();

            ko.applyBindings(_this, _this.ux);
        };
        this.postBound = function (element) {
            g_ViewModel.updatePosition();
        };
        this.edge = this;
        this.comment = comment;
        this.id = "Edge-" + this.comment.id;

        ko.es5.mapping.track(this);
        this.show();
    }
    return CommentEdge;
})();
