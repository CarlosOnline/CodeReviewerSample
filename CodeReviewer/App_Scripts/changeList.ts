/// <reference path="references.ts" />

class ChangeList {
    file: ChangeFile = null;
    changeFiles: Array<ChangeFile> = [];

    dispose = () => {
        this.file = null;
        this.changeFiles.removeAll();
    };

    findChangeFile = (fileName): ChangeFile => {
        return this.changeFiles.first((changeFile) => {
            return changeFile.data.serverFileName == fileName;
        });
    };

    findChangeFileById = (id) => {
        return this.changeFiles.first((changeFile) => {
            return changeFile.data.id == id;
        });
    };

    constructor(public data: Dto.ChangeListDto, private viewModel: ViewModel.Diff) {
        console.assert(this.data.changeFiles.length > 0);

        this.data.changeFiles.forEach((changeFile) => {
            var obj = new ChangeFile(changeFile, this.viewModel);
            this.changeFiles.push(obj);
        });

        if (this.file == null) {
            this.file = this.changeFiles[0];
        }

        ko.es5.mapping.track(this, "ChangeList");
    }
}
