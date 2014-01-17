/// <reference path="references.ts" />

var MAX_INT = 4294967295;

interface KnockoutStatic {
    unapplyBindings: (node: JQuery, remove: boolean) => void;
};

ko.unapplyBindings = function ($node, remove) {
    // unbind events
    $node.find("*").each(function () {
        $(this).unbind();
    });

    // Remove KO subscriptions and references
    if (remove) {
        ko.removeNode($node[0]);
    } else {
        ko.cleanNode($node[0]);
    }
};

function purge(d) {
    var a = d.attributes, i, l, n;
    if (a) {
        for (i = a.length - 1; i >= 0; i -= 1) {
            n = a[i].name;
            if (typeof d[n] === 'function') {
                d[n] = null;
            }
        }
    }
    a = d.childNodes;
    if (a) {
        l = a.length;
        for (i = 0; i < l; i += 1) {
            purge(d.childNodes[i]);
        }
    }
}

function scrollToMiddle(container: JQuery, elem: HTMLElement) {
    if (container == null || elem == null)
        throw "Can scrollToMiddle - null container/elem";

    container.animate({
        scrollTop: elem.offsetTop - container.height() / 3
    }, 250);
}

function hasVerticalScrollBar(elem: Element) {
    return (elem.clientHeight < elem.scrollHeight);
}

function hasHorizontalScrollBar(elem: Element) {
    return (elem.clientWidth < elem.scrollWidth);
}

function ko_MergeArray(src: KnockoutObservableArray<any>, tgt) {
    var newItems = _.difference(tgt, src());
    newItems.forEach(function (item) {
        src.push(item);
    });

    var deletedItems = _.difference(src(), tgt);
    deletedItems.forEach(function (item) {
        src.remove(item);
    });
    return this;
}

function ko_MergeArrayES5(src: Array, tgt) {
    var newItems = _.difference(tgt, src);
    newItems.forEach(function (item) {
        src.push(item);
    });

    var deletedItems = _.difference(src, tgt);
    deletedItems.forEach(function (item) {
        src.unshift(item);
    });
    return this;
}

function getTable(target: HTMLElement) {
    return <HTMLTableElement> getTag(target, "table");
}

function getTR(target: HTMLElement) {
    return <HTMLTableRowElement> getTag(target, "tr");
}

function getTD(target: HTMLElement) {
    return <HTMLTableCellElement> getTag(target, "td");
}

function getTag(target: HTMLElement, tag: string) {
    tag = tag.toLowerCase();
    while (target != null) {
        if (target.tagName.toLowerCase() == tag)
            return <HTMLElement> target;
        target = <HTMLElement> target.parentElement;
    }
    return null;
}

function getElement(target: HTMLElement, tag: string, id?: string): any {
    tag = tag.toLowerCase();
    while (target != null) {
        if (target.tagName.toLowerCase() == tag)
            return target;
        target = target.parentElement;
    }

    id = id || null;
    if (id != null) {
        return document.getElementById(id);
    }
    return null;
}

function getPosition(obj: Element) {
    var curleft = 0;
    var curtop = 0;
    var elem = <HTMLElement> obj;
    var objx = <any> obj;
    if (elem.offsetLeft) curleft += elem.offsetLeft;
    if (elem.offsetTop) curtop += elem.offsetTop;
    if (obj.scrollTop && obj.scrollTop > 0) curtop -= obj.scrollTop;
    if (elem.offsetParent) {
        var pos = getPosition(elem.offsetParent);
        curleft += pos[0];
        curtop += pos[1];
    } else if (objx.ownerDocument) {
        var thewindow = <Window> objx.ownerDocument.defaultView;
        if (!thewindow && objx.ownerDocument.parentWindow)
            thewindow = objx.ownerDocument.parentWindow;
        if (thewindow) {
            if (thewindow.frameElement) {
                var pos2 = getPosition(thewindow.frameElement);
                curleft += pos2[0];
                curtop += pos2[1];
            }
        }
    }

    return [curleft, curtop];
}

class Logger {
    static console = {
        log: <Function> null,
    };

    static getFunctionName(f) {
        if (f) {
            var name = f.toString().substr(9);
            name = name.substr(0, name.indexOf('('));

            if (name) {
                return name;
            }
            else {
                //var caller = Logger.getFunctionName(arguments.callee.caller.caller);
                return '.anonymous';
            }
        }
        return 'null';
    }

    static initLogger() {
        if (window.console && console.log) {
            var old = console.log;
            console.log = function () {
                var func = Logger.getFunctionName(arguments.callee.caller);
                Array.prototype.unshift.call(arguments, "[" + func + "]");
                old.apply(this, arguments)
            }
        }
    }
}
