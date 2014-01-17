/// <reference path="references.ts" />

module Resource {
    export module Types {
        export class ChangeFile {
            id: number;
            value: string;
            icon: {
                classes: string;
                title: string;
            };
            file: {
                classes: string;
            };
        }

        export class Comment {
            value: string;
            QTip: {
                styles: Array<string>;
            };
            Status: {
                list: Array<string>;
            };
        }

        export class CommentStatus {
            id: number;
            value: string;
            key: string;
            icon: {
                classes: string;
                title: string;
            };
            qtip: {
                classes: string;
            };
        }

        export class FileVersion {
            id: number;
            key: string;
            value: string;
        }

        export class Review {
            value: string;
            Status: {
                list: Array<string>;
            };
        }

        export module Reviewer {
            export class Status {
                id: number;
                value: string;
                key: string;
                icon: {
                    classes: string
                }
            }
        }

        export class Stage {
            id: number;
            key: string;
            value: string;
            icon: {
                classes: string;
                title: string;
            }
        }
    }

    export function getResourceFromHash<T>(item, hash, typeName): T {
        for (var key in hash) {
            var value = hash[key];
            if (value.id == item || value.name == item || value.key == item || value.value == item)
                return <T> value;
        }
        var value = hash["Default"];
        if (value !== undefined) {
            return <T> value;
        }

        console.log("Did not find resource: " + item + " in type: " + typeName);
        console.trace();
        throw Error("Did not find resource: " + item + " in type: " + typeName);
    }

    export module ChangeList {
        export function getResource(item): Resource.Types.ChangeFile {
            return getResourceFromHash<Resource.Types.ChangeFile>(item, Resources.ChangeFile, "ChangeFile");
        }
    }

    export module Comment {
        export function getResource(): Resource.Types.Comment {
            return <any> Resources.Comment;
        }
    }

    export module CommentStatus {
        export function getResource(item: any): Resource.Types.CommentStatus {
            return getResourceFromHash<Resource.Types.CommentStatus>(item, Resources.Comment.Status, "Comment");
        }
    }

    export module FileVersion {
        export function getResource(item: any): Resource.Types.FileVersion {
            return getResourceFromHash<Resource.Types.FileVersion>(item, Resources.FileVersion, "Action");
        }
    }

    export module Review {
        export function getResource(): Resource.Types.Review {
            return <any> Resources.Review;
        }
    }

    export module Reviewer {
        export module Status {
            export function getResource(item): Resource.Types.Reviewer.Status {
                return getResourceFromHash<Resource.Types.Reviewer.Status>(item, Resources.Reviewer.Status, "Reviewer.Status");
            }
        }
    }

    export module Stage {
        export function getResource(item): Resource.Types.Stage {
            return getResourceFromHash<Resource.Types.Stage>(item, Resources.Stage, "Stage");
        }
    }
}
