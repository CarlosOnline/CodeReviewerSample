var g_BaseUrl = getBaseUrl();
var g_UserName = getUserName();
var g_ReviewerAlias = getReviewerAlias();
var g_DisplayName = getDisplayName();
var g_ChangeList = null;
var g_ChangeListSettings = null;
var g_UserSettings = null;
var g_ViewModel = null;
var g_AllModel = null;
var g_SubmitChangeListsModel = null;

var Extensions;
(function (Extensions) {
    (function (ArrayExt) {
        function first(callback, thisArg) {
            thisArg = thisArg || this;
            var found = thisArg.filter(callback);
            if (found && found.length > 0)
                return found[0];
            return null;
        }
        ArrayExt.first = first;

        function removeAll(thisArg) {
            thisArg = thisArg || this;
            while (thisArg.length > 0)
                thisArg.pop();
        }
        ArrayExt.removeAll = removeAll;

        // TODO: Fix?
        function remove(elem, thisArg) {
            thisArg = thisArg || this;
            var idx = thisArg.indexOf(elem);
            if (idx >= 0 && idx < thisArg.length)
                thisArg.slice(idx, 1);
        }
        ArrayExt.remove = remove;

        Array.prototype.first = ArrayExt.first;

        //Array.prototype.remove = ArrayExt.remove;
        Array.prototype.removeAll = ArrayExt.removeAll;
    })(Extensions.ArrayExt || (Extensions.ArrayExt = {}));
    var ArrayExt = Extensions.ArrayExt;
})(Extensions || (Extensions = {}));
