angular.module('umbraco').controller("Medium.Dialog", mediumDialogController);
function mediumDialogController($scope, $routeParams, $http, contentResource) {
    var dialogOptions = $scope.dialogOptions;
    var node = dialogOptions.currentNode;

    // var url = Umbraco.Sys.ServerVariables["articulate"]["articulatePropertyEditorsBaseUrl"] + "GetThemes"; See https://our.umbraco.org/documentation/extending/version7-assets
    $scope.busy = true;
    $http.get('/umbraco/backoffice/PostToMedium/PostToMediumApi/InitDialog').success(function (data) {
        console.log(data);
        $scope.MediumDialogModel = data;
        $scope.busy = false;
    }).error(function (data) {
        console.log(data);
        $scope.MediumDialogModel = { ErrorMessage: "Unhandled error - see console for details" };
        $scope.busy = false;
    });

    $scope.authenticateWithMedium = function () {
        window.open($scope.MediumDialogModel.AuthUrl);
    }

    $scope.postToMedium = function () {
        $scope.busy = true;
        $http.get('/umbraco/backoffice/PostToMedium/PostToMediumApi/PostToMedium?nodeId=' + node.id).success(function (data) {
            console.log(data);
            $scope.MediumDialogModel = data;
            $scope.busy = false;
        }).error(function (data) {
            console.log(data);
            $scope.MediumDialogModel = { ErrorMessage: "Unhandled error - see console for details" };
            $scope.busy = false;
        });
    }
}