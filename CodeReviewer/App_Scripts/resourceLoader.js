/// <reference path="references.ts" />
var Resource;
(function (Resource) {
    (function (Types) {
        var ChangeFile = (function () {
            function ChangeFile() {
            }
            return ChangeFile;
        })();
        Types.ChangeFile = ChangeFile;

        var Comment = (function () {
            function Comment() {
            }
            return Comment;
        })();
        Types.Comment = Comment;

        var CommentStatus = (function () {
            function CommentStatus() {
            }
            return CommentStatus;
        })();
        Types.CommentStatus = CommentStatus;

        var FileVersion = (function () {
            function FileVersion() {
            }
            return FileVersion;
        })();
        Types.FileVersion = FileVersion;

        var Review = (function () {
            function Review() {
            }
            return Review;
        })();
        Types.Review = Review;

        (function (Reviewer) {
            var Status = (function () {
                function Status() {
                }
                return Status;
            })();
            Reviewer.Status = Status;
        })(Types.Reviewer || (Types.Reviewer = {}));
        var Reviewer = Types.Reviewer;

        var Stage = (function () {
            function Stage() {
            }
            return Stage;
        })();
        Types.Stage = Stage;
    })(Resource.Types || (Resource.Types = {}));
    var Types = Resource.Types;

    function getResourceFromHash(item, hash, typeName) {
        for (var key in hash) {
            var value = hash[key];
            if (value.id == item || value.name == item || value.key == item || value.value == item)
                return value;
        }
        var value = hash["Default"];
        if (value !== undefined) {
            return value;
        }

        console.log("Did not find resource: " + item + " in type: " + typeName);
        console.trace();
        throw Error("Did not find resource: " + item + " in type: " + typeName);
    }
    Resource.getResourceFromHash = getResourceFromHash;

    (function (ChangeList) {
        function getResource(item) {
            return Resource.getResourceFromHash(item, Resources.ChangeFile, "ChangeFile");
        }
        ChangeList.getResource = getResource;
    })(Resource.ChangeList || (Resource.ChangeList = {}));
    var ChangeList = Resource.ChangeList;

    (function (Comment) {
        function getResource() {
            return Resources.Comment;
        }
        Comment.getResource = getResource;
    })(Resource.Comment || (Resource.Comment = {}));
    var Comment = Resource.Comment;

    (function (CommentStatus) {
        function getResource(item) {
            return Resource.getResourceFromHash(item, Resources.Comment.Status, "Comment");
        }
        CommentStatus.getResource = getResource;
    })(Resource.CommentStatus || (Resource.CommentStatus = {}));
    var CommentStatus = Resource.CommentStatus;

    (function (FileVersion) {
        function getResource(item) {
            return Resource.getResourceFromHash(item, Resources.FileVersion, "Action");
        }
        FileVersion.getResource = getResource;
    })(Resource.FileVersion || (Resource.FileVersion = {}));
    var FileVersion = Resource.FileVersion;

    (function (Review) {
        function getResource() {
            return Resources.Review;
        }
        Review.getResource = getResource;
    })(Resource.Review || (Resource.Review = {}));
    var Review = Resource.Review;

    (function (Reviewer) {
        (function (Status) {
            function getResource(item) {
                return Resource.getResourceFromHash(item, Resources.Reviewer.Status, "Reviewer.Status");
            }
            Status.getResource = getResource;
        })(Reviewer.Status || (Reviewer.Status = {}));
        var Status = Reviewer.Status;
    })(Resource.Reviewer || (Resource.Reviewer = {}));
    var Reviewer = Resource.Reviewer;

    (function (Stage) {
        function getResource(item) {
            return Resource.getResourceFromHash(item, Resources.Stage, "Stage");
        }
        Stage.getResource = getResource;
    })(Resource.Stage || (Resource.Stage = {}));
    var Stage = Resource.Stage;
})(Resource || (Resource = {}));
