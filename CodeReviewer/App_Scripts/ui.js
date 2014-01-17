/// <reference path="references.ts" />
var UI;
(function (UI) {
    UI.BodyContent = null;
    var BodyDelta = $(window).height() - $(document.body).height();

    function onLoad() {
        UI.BodyContent = $("#reviewPane");
        Revisions.onLoad();
        QTip.onLoad();
    }
    UI.onLoad = onLoad;

    function onUnLoad() {
        UI.BodyContent = null;
        Revisions.onUnLoad();
    }
    UI.onUnLoad = onUnLoad;

    function onWindowResize() {
        console.log("onWindowResize");
        if (UI.BodyContent == null) {
            onLoad();
        }
        Revisions.onWindowResize();
        QTip.onWindowResize();
    }
    UI.onWindowResize = onWindowResize;

    function createElement(templateId, id, holder) {
        holder = holder || document.getElementById("templateHolder");

        var template = document.getElementById(templateId);
        $(template.innerHTML).clone().attr("id", id).insertAfter(holder);
        return document.getElementById(id);
        /* jquery-2
        var newElem = <HTMLElement> $(holder).append(template.innerHTML)[0].firstElementChild;
        newElem.id = id;
        return newElem;
        */
    }
    UI.createElement = createElement;

    (function (Effects) {
        function flash(elem) {
            var jq = $(elem);

            jq.toggleClass("highlight", true, 1000, "swing", function () {
                jq.toggleClass("highlight", false, 1000);
            });
        }
        Effects.flash = flash;

        function hide(elem, done) {
            if (typeof done === "undefined") { done = null; }
            done = done || function () {
            };
            var jq = $(elem);
            jq.fadeOut("fast", done);
        }
        Effects.hide = hide;

        function show(elem, done) {
            if (typeof done === "undefined") { done = null; }
            done = done || function () {
            };
            var jq = $(elem);
            jq.fadeIn("fast", done);
        }
        Effects.show = show;
    })(UI.Effects || (UI.Effects = {}));
    var Effects = UI.Effects;

    (function (ContextMenu) {
        function init() {
            $.contextMenu({
                // define which elements trigger this menu
                selector: ".EdgeIcon",
                // define the elements of the menu
                items: {
                    show: { name: "Show Comment", callback: function (key, opt) {
                            g_ViewModel.showComment(opt.$trigger[0]);
                        } },
                    showAll: { name: "Show All Comments", callback: function (key, opt) {
                            alert("Bar!");
                        } }
                }
            });
        }
        ContextMenu.init = init;
    })(UI.ContextMenu || (UI.ContextMenu = {}));
    var ContextMenu = UI.ContextMenu;

    (function (LeftPane) {
        function init(width, closed) {
            closed = closed || false;
            UI.LeftPane.width = width;
            if (closed) {
                $("#leftPane").hide();
                $("#leftPaneLabel").show();
                $("#leftPaneCell").width($("#leftPaneLabel").height());
            } else {
                $("#leftPaneLabel").hide();
                $("#leftPane").show();
                $("#leftPaneCell").width($("#leftPane").width());
            }
        }
        LeftPane.init = init;

        function close() {
            $("#leftPane").animate({ width: 'toggle' });
            $("#leftPaneCell").width($("#leftPaneLabel").height());
            $("#leftPaneLabel").fadeIn();
            Resize.disable();
        }
        LeftPane.close = close;

        function show() {
            $("#leftPaneLabel").fadeOut();
            $("#leftPane").animate({ width: 'toggle' });
            $("#leftPaneCell").width($("#leftPane").width());
            Resize.enable();
        }
        LeftPane.show = show;

        // property width
        LeftPane.width;
        Object.defineProperty(LeftPane, "width", {
            get: function () {
                return $("#leftPane").width();
            },
            set: function (width) {
                if (width > 0) {
                    $("#leftPane").width(width);
                }
                return $("#leftPane").width();
            }
        });

        (function (Resize) {
            var resizable = null;
            function init(callback) {
                callback = callback || function (newWidth, event, ui) {
                };

                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable = new Resizable.TableColumn(td, function (event, ui) {
                    var left = leftPane.offset().left;
                    var newWidth = ui.size.width - left;
                    leftPane.width(newWidth);
                    callback(newWidth, event, ui);
                });
            }
            Resize.init = init;

            function enable() {
                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable.enable(td);
            }
            Resize.enable = enable;

            function disable() {
                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable.disable(td);
            }
            Resize.disable = disable;
        })(LeftPane.Resize || (LeftPane.Resize = {}));
        var Resize = LeftPane.Resize;
    })(UI.LeftPane || (UI.LeftPane = {}));
    var LeftPane = UI.LeftPane;

    (function (QTip) {
        QTip.minLeft = 0;

        function onLoad() {
            onWindowResize();
        }
        QTip.onLoad = onLoad;

        function add(target, tipContents, status, renderCallback, showCallback) {
            renderCallback = renderCallback || function () {
            };
            showCallback = showCallback || function () {
            };
            status = status || "";

            var jqTarget = $(target);
            var container = getTag(getTag(target, "table"), "div");
            jqTarget.qtip({
                prerender: true,
                content: $(tipContents),
                position: {
                    my: "top left",
                    at: "bottom left",
                    of: target,
                    adjust: {
                        y: -($(target).height()),
                        x: UI.QTip.minLeft,
                        screen: true,
                        method: "shift none",
                        resize: true
                    },
                    container: $(container),
                    viewport: $(container)
                },
                show: {
                    delay: 0,
                    event: false,
                    ready: 'false'
                },
                hide: {
                    event: false,
                    fixed: false
                },
                events: {
                    render: function (event, api) {
                        renderCallback(event, api);
                    },
                    visible: function (event, api) {
                        showCallback(event, api);
                    }
                }
            });
            return jqTarget.qtip();
        }
        QTip.add = add;

        function set(qtipElem, data) {
            $(qtipElem).qtip("api").set(data);
        }
        QTip.set = set;

        function showAll(state) {
            state = state != undefined ? state : true;
            var show = state ? 'show' : 'hide';
            $('.qtip').each(function () {
                $(this).qtip(show);
            });
        }
        QTip.showAll = showAll;

        function getLeft(qtipElem) {
            var jq = $(qtipElem);
            var qWidth = jq.width();
            var api = jq.qtip('api');
            var target = api.get('position.of');
            var tOffset = $(target).position().left;
            var adjustX = $(UI.Revisions.Current.RightTable).width() - qWidth - tOffset - Resources.Constants.Comment.PaddingRight;
            if (adjustX < QTip.minLeft)
                adjustX = QTip.minLeft;
            return adjustX;
        }
        QTip.getLeft = getLeft;

        function resizeAll() {
            $('.qtip').each(function () {
                reposition(this);
            });
        }

        function reposition(elem) {
            var adjustX = getLeft(elem);
            var api = $(elem).qtip('api');
            api.set({ 'position.adjust.x': adjustX });
        }
        QTip.reposition = reposition;

        function moveQtip(elem, target, left, top) {
            $(elem).position({
                of: $(target),
                my: 'left top',
                at: 'left bottom',
                offset: left + ' ' + top
            });
        }
        QTip.moveQtip = moveQtip;

        function onWindowResize() {
            if (UI.Revisions.Current == null && UI.Revisions.Current.RightTable == null)
                return;

            var container = $(UI.Revisions.Current.RightTable);
            var width = container.width();
            UI.QTip.minLeft = Math.max(Resources.Constants.Comment.MinLeft, width / 2);

            resizeAll();
        }
        QTip.onWindowResize = onWindowResize;

        function setClass(elem, classes) {
            var jq = $(elem);
            Resource.Comment.getResource().QTip.styles.forEach(function (style) {
                if (-1 != elem.className.indexOf(style))
                    jq.removeClass(style);
            });
            jq.addClass(classes);
        }
        QTip.setClass = setClass;
    })(UI.QTip || (UI.QTip = {}));
    var QTip = UI.QTip;

    (function (Revisions) {
        Revisions.All = [];
        Revisions.Current = null;
        var div = {
            DiffTables: null,
            LeftTables: null,
            RightTables: null,
            EdgeTables: null
        };

        function getPartnerTable(elem) {
            var table = getTable(elem);
            var container = getTD(table);

            var partnerCell = container.cellIndex == 0 ? container.nextElementSibling : container.previousElementSibling;
            var partnerTable = partnerCell.firstElementChild.firstElementChild;

            return partnerTable;
        }
        Revisions.getPartnerTable = getPartnerTable;

        function onLoad() {
            div.DiffTables = $("div.DiffTable");
            div.LeftTables = $("div.LeftTable");
            div.RightTables = $("div.RightTable");
            div.EdgeTables = $("div.EdgeTable");

            if (div.DiffTables.length == 0)
                throw "Error missing div.DiffTables";

            Revisions.All = [];
            $("div.FileDiffs").each(function (index, element) {
                var revision = {
                    DiffTable: $(this).find("div.DiffTable"),
                    LeftTable: $(this).find("div.LeftTable"),
                    RightTable: $(this).find("div.RightTable"),
                    EdgeTable: $(this).find("div.EdgeTable"),
                    Tables: {
                        LeftTable: $(this).find("table.LeftTable"),
                        RightTable: $(this).find("table.RightTable"),
                        EdgeTable: $(this).find("table.EdgeTable")
                    }
                };
                Revisions.All.push(revision);
            });

            onRevisionChange(g_ViewModel.revision.tabIndex);
        }
        Revisions.onLoad = onLoad;

        function onUnLoad() {
            div.LeftTables = null;
            div.RightTables = null;
            div.EdgeTables = null;
            Revisions.All = [];
            Revisions.Current = null;
        }
        Revisions.onUnLoad = onUnLoad;

        function onWindowResize() {
            if (UI.BodyContent == null)
                return;

            function verticalResize(div, newHeight) {
                if (div == null || div.length <= 0)
                    return;
                div.height(newHeight);
            }

            var headerHeight = $(".header").height();
            var newHeight = $(window).height() - UI.BodyContent.offset().top - headerHeight;
            verticalResize(div.LeftTables, newHeight);
            verticalResize(div.RightTables, newHeight);
            verticalResize(div.EdgeTables, newHeight);

            // Run a 2nd time b/c the first time changes the BodyContent.offset().top
            newHeight = $(window).height() - UI.BodyContent.offset().top - headerHeight;
            verticalResize(div.LeftTables, newHeight);
            verticalResize(div.RightTables, newHeight);
            verticalResize(div.EdgeTables, newHeight);
        }
        Revisions.onWindowResize = onWindowResize;

        function onRevisionChange(index) {
            Revisions.Current = null;
            if (Revisions.All.length <= 0 || index >= Revisions.All.length)
                return;

            Revisions.Current = Revisions.All[index];
        }
        Revisions.onRevisionChange = onRevisionChange;

        (function (Tabs) {
            var resizables = [];

            function load(tabIndex, selected, activated) {
                activated = activated || function () {
                    return false;
                };
                selected = selected || function () {
                    return false;
                };

                var tables = $("div.LeftTable");
                for (var idx = 0; idx < tables.length; idx++) {
                    var table = tables[idx];
                    var td = getTD(table);
                    resizables.push(new Resizable.TableColumn(td, null, "leftTableResizable"));
                }
                ;

                $("#tabs").tabs({
                    // BUG: fx do not expand tabs on content change.
                    // fx: [
                    //       { opacity: 'toggle', duration: 'fast' },
                    //       { opacity: 'toggle', duration: 'normal' } ],
                    select: function (event, ui) {
                        //console.log("Tabs", "selected");
                        return selected(ui.index);
                    },
                    selected: tabIndex,
                    show: function (event, ui) {
                        //console.log("Tabs", "shown");
                        return activated(ui.index);
                    }
                });
            }
            Tabs.load = load;

            function visible() {
                return $("#tabs").is(":visible");
            }
            Tabs.visible = visible;

            function show(done) {
                //$("#tabs").show();
                $("#tabs").fadeIn("fast", function () {
                    //setTimeout(() => {
                    resizables.forEach(function (resizable) {
                        resizable.load();
                        //});
                    }, 100);
                    if (done != undefined)
                        done();
                });
                return;
            }
            Tabs.show = show;

            function hide(done) {
                // Timing issues with show.  fadeOut without callback can interfere with fadeIn with callback.
                // Until I use a callback, continue to use hide
                $("#tabs").fadeOut("fast", done);
            }
            Tabs.hide = hide;

            function destroy() {
                $("#tabs").tabs("destroy");
            }
            Tabs.destroy = destroy;

            function select(index) {
                //console.log("Tabs", "select");
                $("#tabs").tabs("select", index);
            }
            Tabs.select = select;

            Tabs.animation = {
                clear: function () {
                    $("#tabs").tabs("option", { fx: [] });
                },
                set: function () {
                    $("#tabs").tabs("option", {
                        fx: [
                            { opacity: 'toggle', duration: 'fast' },
                            { opacity: 'toggle', duration: 'normal' }
                        ]
                    });
                }
            };
        })(Revisions.Tabs || (Revisions.Tabs = {}));
        var Tabs = Revisions.Tabs;
    })(UI.Revisions || (UI.Revisions = {}));
    var Revisions = UI.Revisions;

    var checkbox = (function () {
        function checkbox() {
            this.value = "";
            this.checked = false;
        }
        return checkbox;
    })();
    UI.checkbox = checkbox;

    var select = (function () {
        function select(value, list, display, callback) {
            if (typeof list === "undefined") { list = []; }
            var _this = this;
            this.display = display;
            this.callback = callback;
            this.value = "";
            this.list = [];
            this.subscription = null;
            this.idx = ko.es5.mapping.property(function () {
                return _this.list.indexOf(_this.value);
            }, function (idx) {
                _this.value = _this.list[idx];
            });
            this.dispose = function () {
                if (_this.subscription != null) {
                    _this.subscription.dispose();
                }
            };
            this.subscribe = function () {
                _this.dispose();
                if (_this.callback == null) {
                    return;
                }

                if (_this.subscription == null)
                    _this.subscription = new Subscription(ko.es5.mapping.getObservable(_this, "value"), _this.callback, false);

                _this.subscription.subscribe();
            };
            this.value = value;
            this.list = list;
            this.display = this.display || function () {
                return true;
            };
            this.callback = this.callback || null;

            ko.es5.mapping.track(this);
        }
        return select;
    })();
    UI.select = select;

    (function (LineStamp) {
        var Side = (function () {
            function Side() {
                this.id = "";
                this.fileVersionId = 0;
                this.lineNumber = "";
                this.lineBase = "";
                this.lineOffset = "";
                this.other = null;
            }
            return Side;
        })();
        LineStamp.Side = Side;

        var Info = (function () {
            function Info() {
                this.id = "";
                this.type = "";
                this.fileVersionId = 1;
                this.reviewRevision = 0;
                this.side = null;
                this.left = new Side();
                this.right = new Side();
            }
            return Info;
        })();
        LineStamp.Info = Info;
    })(UI.LineStamp || (UI.LineStamp = {}));
    var LineStamp = UI.LineStamp;

    function defaultErrorMessage(title, errorHtml) {
        title = title || "Unknown Error";
        console.log(title, errorHtml);
        if (errorHtml.length == 0)
            return;

        var jqDialog = $("#dialog");
        jqDialog[0].innerHTML = errorHtml;
        jqDialog[0].title = title;
        jqDialog.css({
            overflow: "auto"
        });

        jqDialog.css({ 'max-width': '800px' });
        jqDialog.css({ 'max-height': '800px' });

        jqDialog.dialog({
            modal: true,
            resize: "auto",
            width: "auto",
            maxWidth: 800,
            height: "auto",
            maxHeight: 800,
            show: "slow",
            open: function (event, ui) {
                UI.QTip.showAll(false);
            },
            close: function (event, ui) {
                UI.QTip.showAll(true);
            },
            buttons: [
                {
                    text: "OK",
                    click: function () {
                        ($(this)).dialog("close");
                    }
                }
            ]
        });
    }
    UI.defaultErrorMessage = defaultErrorMessage;

    function Error(title, errorHtml) {
        setTimeout(function () {
            defaultErrorMessage(title, "<p>" + errorHtml + "</p>");
        }, 0);
    }
    UI.Error = Error;
})(UI || (UI = {}));
