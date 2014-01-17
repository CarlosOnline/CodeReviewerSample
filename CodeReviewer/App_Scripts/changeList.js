/// <reference path="references.ts" />
var ChangeList = (function () {
    function ChangeList(data, viewModel) {
        var _this = this;
        this.data = data;
        this.viewModel = viewModel;
        this.file = null;
        this.changeFiles = [];
        this.dispose = function () {
            _this.file = null;
            _this.changeFiles.removeAll();
        };
        this.findChangeFile = function (fileName) {
            return _this.changeFiles.first(function (changeFile) {
                return changeFile.data.serverFileName == fileName;
            });
        };
        this.findChangeFileById = function (id) {
            return _this.changeFiles.first(function (changeFile) {
                return changeFile.data.id == id;
            });
        };
        console.assert(this.data.changeFiles.length > 0);

        this.data.changeFiles.forEach(function (changeFile) {
            var obj = new ChangeFile(changeFile, _this.viewModel);
            _this.changeFiles.push(obj);
        });

        if (this.file == null) {
            this.file = this.changeFiles[0];
        }

        ko.es5.mapping.track(this, "ChangeList");
    }
    return ChangeList;
})();
