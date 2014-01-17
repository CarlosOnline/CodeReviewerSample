var Resources = {
    Constants: {
        Comment: {
            MinLeft: 100,
            PaddingRight: 20,
        },
    },

    ChangeFile: {
        Active: {
            id: 0,
            value: "Active",
            icon: {
                classes: "ui-icon ui-icon-comment",
                title: "Pending comments",
            },
            file: {
                classes: "Active",
            },
        },
        Resolved: {
            id: 1,
            value: "Resolved",
            icon: {
                classes: "ui-icon ui-icon-check",
                title: "Resolved comments",
            },
            file: {
                classes: "Resolved",
            },
        },
        WontFix: {
            id: 2,
            value: "WontFix",
            icon: {
                classes: "ui-icon ui-icon-cancel",
                title: "Won't fix comments",
            },
            file: {
                classes: "WontFix",
            },
        },
        Closed: {
            id: 3,
            value: "Closed",
            icon: {
                classes: "ui-icon ui-icon-circle-check",
                title: "Closed comments",
            },
            file: {
                classes: "Closed",
            },
        },
        Canceled: {
            id: 4,
            value: "Canceled",
            icon: {
                classes: "ui-icon ui-icon-battery-0",
                title: "No comments",
            },
            file: {
                classes: "",
            },
        },
        None: {
            id: 5,
            value: "None",
            icon: {
                classes: "ui-icon ui-icon-battery-0",
                title: "No comments",
            },
            file: {
                classes: "",
            },
        },
    },

    Comment: {
        QTip: {
            styles: [
                "qtip-default",
                "qtip-green",
                "qtip-blue",
                "qtip-closed",
            ],
        },
        Status: {
            value: "Active",
            list: [
                "Active",
                "Resolved",
                "Won't Fix",
                "Closed"
            ],
            Active: {
                id: 0,
                key: "Active",
                value: "Active",
                icon: {
                    classes: "ui-icon ui-icon-comment",
                    title: "Active",
                },
                qtip: {
                    classes: "qtip-default qtip-rounded qtip-shadow Active",
                },
            },
            Resolved: {
                id: 1,
                key: "Resolved",
                value: "Resolved",
                icon: {
                    classes: "ui-icon ui-icon-check Resolved",
                    title: "Resolved",
                },
                qtip: {
                    classes: "qtip-green qtip-rounded qtip-shadow",
                },
            },
            WontFix: {
                id: 2,
                key: "WontFix",
                value: "Won't Fix",
                icon: {
                    classes: "ui-icon ui-icon-cancel WontFix",
                    title: "Won't Fix",
                },
                qtip: {
                    classes: "qtip-blue qtip-rounded qtip-shadow WontFix",
                },
            },
            Closed: {
                id: 3,
                key: "Closed",
                value: "Closed",
                icon: {
                    classes: "ui-icon ui-icon-circle-check Closed",
                    title: "Closed",
                },
                qtip: {
                    classes: "qtip-closed qtip-rounded qtip-shadow Closed",
                },
            },
            Canceled: {
                id: 4,
                key: "Canceled",
                value: "Canceled",
                icon: {
                    classes: "",
                    title: "Canceled",
                },
                qtip: {
                    classes: "",
                },
            },
            Default: {
                id: -1,
                key: "Default",
                value: "Default",
                icon: {
                    classes: "ui-icon ui-icon-comment Active",
                    title: "",
                },
                qtip: {
                    classes: "qtip-default qtip-rounded qtip-shadow Active",
                },
            },
        },
    },

    Review: {
        value: "Active",
        Status: {
            list: [
                "Active",
                "Resolved",
                "Closed",
                "Deleted"
            ],
        },
    },

    Reviewer: {
        Status: {
            NotLookedAtYet: {
                id: 0,
                value: "Not looked at",
                key: "NotLookedAtYet",
                icon: {
                    classes: " ui-icon ui-icon-comment NotLookedAtYet",
                },
            },
            Looking: {
                id: 1,
                value: "Looking",
                key: "Looking",
                icon: {
                    classes: " ui-icon ui-icon-comment Looking",
                },
            },
            SignedOff: {
                id: 2,
                value: "Signed off",
                key: "SignedOff",
                icon: {
                    classes: " ui-icon ui-icon-comment SignedOff",
                },
            },
            SignedOffWithComments: {
                id: 3,
                value: "Signed off with comments",
                key: "SignedOffWithComments",
                icon: {
                    classes: " ui-icon ui-icon-comment SignedOffWithComments",
                },
            },
            WaitingOnAuthor: {
                id: 4,
                value: "Waiting on author",
                key: "WaitingOnAuthor",
                icon: {
                    classes: " ui-icon ui-icon-comment WaitingOnAuthor",
                },
            },
            Complete: {
                id: 5,
                value: "Complete",
                key: "Complete",
                icon: {
                    classes: " ui-icon ui-icon-comment Complete",
                },
            },
            None: {
                id: 6,
                value: "",
                key: "None",
                icon: {
                    classes: " ui-icon ui-icon-comment None",
                },
            },
        },
    },

    Stage: {
        Active: {
            id: 0,
            value: "Active",
            key: "Active",
            icon: {
                classes: "ui-icon ui-icon-comment",
                title: "Active",
            },
        },
        Resolved: {
            id: 1,
            value: "Resolved",
            key: "Resolved",
        },
        Closed: {
            id: 2,
            value: "Closed",
            key: "Closed",
            icon: {
                classes: "ui-icon ui-icon-comment",
                title: "Closed",
            },
        },
        Deleted: {
            id: 3,
            value: "Deleted",
            key: "Deleted",
            icon: {
                classes: "ui-icon ui-icon-comment",
                title: "Deleted",
            },
        },
    },

    FileVersion: {
        Add: {
            id: 0,
            key: "Add",
            value: "Add",
        },
        Edit: {
            id: 1,
            key: "Edit",
            value: "Edit",
        }, 
        Delete: {
            id: 2,
            key: "Delete",
            value: "Delete",
        },
        Branch: {
            id: 3,
            key: "Branch",
            value: "Branch",
        },
        Integrate: {
            id: 4,
            key: "Integrate",
            value: "Integrate",
        },
        Rename: {
            id: 5,
            key: "Rename",
            value: "Rename",
        },
    },
}
