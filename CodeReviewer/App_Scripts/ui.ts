/// <reference path="references.ts" />

interface JQuery {
    qtip: any;
}

module UI {
    export var BodyContent = null;
    var BodyDelta = $(window).height() - $(document.body).height();

    export function onLoad() {
        BodyContent = $("#reviewPane");
        Revisions.onLoad();
        QTip.onLoad();
    }

    export function onUnLoad() {
        BodyContent = null;
        Revisions.onUnLoad();
    }

    export function onWindowResize() {
        console.log("onWindowResize");
        if (BodyContent == null) {
            onLoad();
        }
        Revisions.onWindowResize();
        QTip.onWindowResize();
    }

    export function createElement(templateId: string, id: string, holder?: HTMLElement) {
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

    export module Effects {
        export function flash(elem) {
            var jq = $(elem);

            jq.toggleClass("highlight", true, 1000, "swing", function () {
                jq.toggleClass("highlight", false, 1000);
            });
        }

        export function hide(elem: HTMLElement, done = null) {
            done = done || function () { };
            var jq = $(elem);
            jq.fadeOut("fast", done);
        }

        export function show(elem: HTMLElement, done = null) {
            done = done || function () { };
            var jq = $(elem);
            jq.fadeIn("fast", done);
        }
    }

    export module ContextMenu {
        export function init() {
            $.contextMenu({
                // define which elements trigger this menu
                selector: ".EdgeIcon",
                // define the elements of the menu
                items: {
                    show: { name: "Show Comment", callback: function (key, opt) { g_ViewModel.showComment(opt.$trigger[0]); } },
                    showAll: { name: "Show All Comments", callback: function (key, opt) { alert("Bar!"); } }
                }
                // there's more, have a look at the demos and docs...
            });
        }
    } // ContextMenu

    export module LeftPane {
        export function init(width: number, closed: boolean) {
            closed = closed || false;
            UI.LeftPane.width = width;
            if (closed) {
                $("#leftPane").hide();
                $("#leftPaneLabel").show();
                $("#leftPaneCell").width($("#leftPaneLabel").height());
            }
            else {
                $("#leftPaneLabel").hide();
                $("#leftPane").show();
                $("#leftPaneCell").width($("#leftPane").width());
            }
        }

        export function close() {
            $("#leftPane").animate({ width: 'toggle' });
            $("#leftPaneCell").width($("#leftPaneLabel").height());
            $("#leftPaneLabel").fadeIn();
            Resize.disable();
        }

        export function show() {
            $("#leftPaneLabel").fadeOut();
            $("#leftPane").animate({ width: 'toggle' });
            $("#leftPaneCell").width($("#leftPane").width());
            Resize.enable();
        }

        // property width
        export var width;
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

        export module Resize {
            var resizable: Resizable.TableColumn = null;
            export function init(callback?: Function) {
                callback = callback || function (newWidth: number, event?: any, ui?: any) {
                }

                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable = new Resizable.TableColumn(td, function (event, ui) {
                    var left = leftPane.offset().left;
                    var newWidth = ui.size.width - left;
                    leftPane.width(newWidth);
                    callback(newWidth, event, ui);
                });
            }

            export function enable() {
                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable.enable(td);
            }

            export function disable() {
                var leftPane = $("#leftPane");
                var td = getTD(leftPane[0]);
                resizable.disable(td);
            }
        }
    } // LeftPane

    export module QTip {
        export var minLeft = 0;

        export function onLoad() {
            onWindowResize();
        }

        export function add(target: HTMLElement, tipContents: HTMLElement, status?: string, renderCallback?: (event, api) => any, showCallback?: (event, api) => any) {
            renderCallback = renderCallback || function () { };
            showCallback = showCallback || function () { };
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
                        y: - ($(target).height()), // bring tips to bottom of line
                        x: UI.QTip.minLeft,
                        screen: true,
                        method: "shift none",
                        resize: true,
                    },
                    container: $(container),
                    viewport: $(container)
                },
                show: {
                    delay: 0,
                    event: false, // disable show on mouseEnter
                    ready: 'false',
                },
                hide: {
                    event: false, // disable hide on mouseLeave
                    fixed: false
                },
                events: {
                    render: function (event, api) {
                        renderCallback(event, api);
                    },
                    visible: function (event, api) {
                        showCallback(event, api);
                    }
                },
                //style: {
                //    classes: classes
                //}
            });
            return jqTarget.qtip();
        }

        export function set(qtipElem: HTMLElement, data: any) {
            $(qtipElem).qtip("api").set(data);
        }

        export function showAll(state: boolean) {
            state = state != undefined ? state : true;
            var show = state ? 'show' : 'hide';
            $('.qtip').each(function () {
                $(this).qtip(show);
            });
        }

        export function getLeft(qtipElem: HTMLElement) {
            var jq = $(qtipElem);
            var qWidth = jq.width();
            var api = jq.qtip('api');
            var target = api.get('position.of');
            var tOffset = $(target).position().left;
            var adjustX = $(UI.Revisions.Current.RightTable).width() - qWidth - tOffset - Resources.Constants.Comment.PaddingRight;
            if (adjustX < minLeft)
                adjustX = minLeft;
            return adjustX;
        }

        function resizeAll() {
            $('.qtip').each(function () {
                reposition(this);
            });
        }

        export function reposition(elem: HTMLElement) {
            var adjustX = getLeft(elem);
            var api = $(elem).qtip('api');
            api.set({ 'position.adjust.x': adjustX });
        }

        export function moveQtip(elem: HTMLElement, target: HTMLElement, left: number, top: number) {
            $(elem).position({
                of: $(target),
                my: 'left top',
                at: 'left bottom',
                offset: left + ' ' + top
            });
        }

        export function onWindowResize() {
            if (UI.Revisions.Current == null && UI.Revisions.Current.RightTable == null)
                return;

            var container = $(UI.Revisions.Current.RightTable);
            var width = container.width();
            UI.QTip.minLeft = Math.max(Resources.Constants.Comment.MinLeft, width / 2);

            resizeAll();
        }

        export function setClass(elem, classes: string) {
            var jq = $(elem);
            Resource.Comment.getResource().QTip.styles.forEach((style) => {
                if (-1 != elem.className.indexOf(style))
                    jq.removeClass(style);
            });
            jq.addClass(classes);
        }
    } // QTip

    export module Revisions {
        export var All = [];
        export var Current = null;
        var div = {
            DiffTables: <JQuery> null,
            LeftTables: <JQuery> null,
            RightTables: <JQuery> null,
            EdgeTables: <JQuery> null
        };

        export function getPartnerTable(elem: HTMLElement) {
            var table = getTable(elem);
            var container = getTD(table);

            var partnerCell = container.cellIndex == 0 ? container.nextElementSibling : container.previousElementSibling;
            var partnerTable = partnerCell.firstElementChild.firstElementChild;

            return <HTMLTableElement> partnerTable;
        }

        export function onLoad() {
            div.DiffTables = $("div.DiffTable");
            div.LeftTables = $("div.LeftTable");
            div.RightTables = $("div.RightTable");
            div.EdgeTables = $("div.EdgeTable");

            if (div.DiffTables.length == 0)
                throw "Error missing div.DiffTables";

            All = [];
            $("div.FileDiffs").each(function (index, element) {
                var revision = {
                    DiffTable: $(this).find("div.DiffTable"),
                    LeftTable: $(this).find("div.LeftTable"),
                    RightTable: $(this).find("div.RightTable"),
                    EdgeTable: $(this).find("div.EdgeTable"),
                    Tables: {
                        LeftTable: $(this).find("table.LeftTable"),
                        RightTable: $(this).find("table.RightTable"),
                        EdgeTable: $(this).find("table.EdgeTable"),
                    }
                };
                All.push(revision);
            });

            onRevisionChange(g_ViewModel.revision.tabIndex);
        }

        export function onUnLoad() {
            div.LeftTables = null;
            div.RightTables = null;
            div.EdgeTables = null;
            All = [];
            Current = null;
        }

        export function onWindowResize() {
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

        export function onRevisionChange(index) {
            Current = null;
            if (All.length <= 0 || index >= All.length)
                return;

            Current = All[index];
        }

        export module Tabs {
            var resizables: Array<Resizable.TableColumn> = [];

            export function load(tabIndex: number,
                selected?: (newIndex: number) => boolean,
                activated?: (newIndex: number) => boolean) {
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
                };

                $("#tabs").tabs({
                    // BUG: fx do not expand tabs on content change.
                    // fx: [
                    //       { opacity: 'toggle', duration: 'fast' },
                    //       { opacity: 'toggle', duration: 'normal' } ],
                    select: (event, ui) => {
                        //console.log("Tabs", "selected");
                        return selected(ui.index);
                    },
                    selected: tabIndex,
                    show: (event, ui) => {
                        //console.log("Tabs", "shown");
                        return activated(ui.index);
                    },
                });
            }

            export function visible() {
                return $("#tabs").is(":visible");
            }

            export function show(done?: () => any) {
                //$("#tabs").show();
                $("#tabs").fadeIn("fast", () => {
                    //setTimeout(() => {
                        resizables.forEach((resizable) => {
                            resizable.load();
                        //});
                    }, 100);
                    if (done != undefined)
                        done();
                }); return;
            }

            export function hide(done?: () => any) {
                // Timing issues with show.  fadeOut without callback can interfere with fadeIn with callback.
                // Until I use a callback, continue to use hide
                $("#tabs").fadeOut("fast", done);
            }

            export function destroy() {
                $("#tabs").tabs("destroy");
            }

            export function select(index) {
                //console.log("Tabs", "select");
                $("#tabs").tabs("select", index);
            }

            export var animation = {
                clear: function () {
                    $("#tabs").tabs("option", { fx: [] });
                },

                set: function () {
                    $("#tabs").tabs("option", {
                        fx: [
                            { opacity: 'toggle', duration: 'fast' },
                            { opacity: 'toggle', duration: 'normal' }]
                    });
                },
            }
        } // UI.Revisions.Tabs
    } // UI.Revisions

    export class checkbox {
        public value = "";
        public checked = false;

    }

    export class select {
        public value = "";
        public list = [];
        public subscription: Subscription = null;

        public idx = ko.es5.mapping.property(() => { return this.list.indexOf(this.value); },
            (idx: number) => { this.value = this.list[idx]; }
            );

        dispose = () => {
            if (this.subscription != null) {
                this.subscription.dispose();
            }
        };

        subscribe = () => {
            this.dispose();
            if (this.callback == null) {
                return;
            }

            if (this.subscription == null)
                this.subscription = new Subscription(ko.es5.mapping.getObservable(this, "value"), this.callback, false);

            this.subscription.subscribe();
        };

        constructor(value: string, list = [], public display?: Function, public callback?: (newValue) => void) {
            this.value = value;
            this.list = list;
            this.display = this.display || () => { return true };
            this.callback = this.callback || null;

            ko.es5.mapping.track(this);
        }
    } // UI.select

    export module LineStamp {
        export class Side {
            id = "";
            fileVersionId = 0;
            lineNumber = "";
            lineBase = "";
            lineOffset = "";
            other: Side = null;
        }

        export class Info {
            id = "";
            type = "";
            fileVersionId = 1;
            reviewRevision = 0;
            side: Side = null;
            left = new Side();
            right = new Side();

            constructor() {
            }
        }
    }

    export function defaultErrorMessage(title: string, errorHtml: string) {
        title = title || "Unknown Error";
        console.log(title, errorHtml);
        if (errorHtml.length == 0)
            return;

        var jqDialog: any = $("#dialog");
        jqDialog[0].innerHTML = errorHtml;
        jqDialog[0].title = title;
        jqDialog.css({
            overflow: "auto",
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
                        (<any>$(this)).dialog("close");
                    }
                }
            ],
        });
    }

    export function Error(title: string, errorHtml: string) {
        setTimeout(function () {
            defaultErrorMessage(title, "<p>" + errorHtml + "</p>");
        }, 0);
    }

} // UI
