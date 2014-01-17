/*
/// <reference path="references.ts" />
/// <reference path="../Scripts/typings/jsplumb/jquery.jsPlumb.d.ts

module JSPlumb {
    export interface jsPlumb {
        setRenderMode(renderMode: string): string;
        bind(event: string, callback: (e) => void): void;
        unbind(event?: string): void;
        ready(callback: () => void): void;
        importDefaults(defaults: Defaults): void;
        Defaults: Defaults;
        restoreDefaults(): void;
        addClass(el: any, clazz: string): void;
        addEndpoint(ep: any, params: any, referenceParam?: any): any;
        removeClass(el: any, clazz: string): void;
        hasClass(el: any, clazz: string): void;
        draggable(el: any, container?: DragContainer): void;
        connect(connection: ConnectParams, referenceParams?: any): void;
        makeSource(el: string, options: SourceOptions): void;
        makeTarget(el: string, options: TargetOptions): void;
        repaintEverything(): void;
        detachEveryConnection(): void;
        detachAllConnections(el: string): void;
        removeAllEndpoints(el: any): void;
        select(params: SelectParams): Connections;
        getConnections(options?: any, flat?: any): any[];
        Endpoints: any;
    }

    export interface Defaults {
        EndpointStyle?: any[];
        PaintStyle?: PaintStyle;
        HoverPaintStyle?: PaintStyle;
        ConnectionsDetachable?: boolean;
        ReattachConnections?: boolean;
        ConnectionOverlays?: any[][];
        Container: any;
    }

    export interface PaintStyle {
        strokeStyle: string;
        lineWidth: number;
    }

    export interface ArrowOverlay {
        location: number;
        id: string;
        length: number;
        foldback: number;
    }

    export interface LabelOverlay {
        label: string;
        id: string;
        location: number;
    }

    export interface Connections {
        detach(): void;
        length: number;
    }

    export interface ConnectParams {
        source: string;
        target: string;
        detachable?: boolean;
        deleteEndpointsOnDetach?: boolean;
        endPoint?: string;
        anchor?: string;
        anchors?: any[];
        label?: string;
    }

    export interface DragContainer {
        containment: string;
    }

    export interface SourceOptions {
        parent: string;
        endpoint?: string;
        anchor?: string;
        connector?: any[];
        connectorStyle?: PaintStyle;
    }

    export interface TargetOptions {
        isTarget?: boolean;
        maxConnections?: number;
        uniqueEndpoint?: boolean;
        deleteEndpointsOnDetach?: boolean;
        endpoint?: string;
        dropOptions?: DropOptions;
        anchor?: any;
    }

    export interface DropOptions {
        hoverClass: string;
    }

    export interface SelectParams {
        scope?: string;
        source: string;
        target: string;
    }
}
declare var jsPlumb: JSPlumb.jsPlumb;

module JSPlumber {
    export function Init() {
        jsPlumb.ready(function () {
        });
    }

    export function connect(left: HTMLElement, right: HTMLElement, cls?: string) {
        // three ways to do this - an id, a list of ids, or a selector (note the two different types of selectors shown here...anything that is valid jquery will work of course)

        jsPlumb.Defaults.Container = $("body");
        jsPlumb.Defaults.EndpointStyle = [{ fillStyle: '#225588' }, { fillStyle: '#558822' }];

        //jsPlumb.draggable(cls);

        var e0 = jsPlumb.addEndpoint(left, null);
        var e1 = jsPlumb.addEndpoint(right, null);

        if (false)
            jsPlumb.connect({
                source: e0,
                target: e1,
                anchors: ["LeftMiddle", "RightMiddle"],
                paintStyle: { strokeStyle: "blue", lineWidth: 1, radius: 2 },

                connector: ["Flowchart", { minStubLength: 1 }]
            });

        jsPlumb.connect({
            source: e0,
            target: e1,
            paintStyle: {
                lineWidth: 10,
                gradient: { stops: [[0, 'blue'], [1, 'red']] }
            },
            endpoints: [new jsPlumb.Endpoints.Dot({ radius: 10 }), new jsPlumb.Endpoints.Dot({ radius: 25 })],
            endpointStyles: [{ fillStyle: 'blue' }, { fillStyle: 'red' }],
            anchors: [[0.333, 1, 0, 1], [0.333, 0, 0, -1]]
        });
    }
}*/