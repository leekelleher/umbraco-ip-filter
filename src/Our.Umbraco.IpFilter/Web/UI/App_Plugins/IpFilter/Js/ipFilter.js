angular.module("umbraco")
    .controller("Our.Umbraco.IpFilter.Controllers.IpFilterController", [
    "$scope",
    "$rootScope",
    "treeService",
    "notificationsService",
    "navigationService",
    "Our.Umbraco.IpFilter.Services.IpFilterService",
    function ($scope, $rootScope, treeService, notificationsService, navigationService, ipFilterService) {

        //console.log($scope);

        var nodeId = $scope.dialogOptions.currentNode.id;
        var nodePath = treeService.getPath($scope.dialogOptions.currentNode);

        $scope.entry = {
            nodeId: nodeId,
            whitelist: "",
            blacklist: "",
            errorPageNodeId: 0,
            enabled: false
        };

        $scope.close = function () {
            UmbClientMgr.closeModalWindow();
        };

        $scope.save = function () {
            ipFilterService.saveEntry($scope.entry).success(function () {
                notificationsService.success("IP Filter settings updated successfully.");
                navigationService.syncTree({
                    tree: 'content',
                    path: nodePath,
                    forceReload: true
                });
                //treeService.reloadNode($scope.dialogOptions.currentNode);
                $scope.close();
            });
        };

        ipFilterService.getEntryByNodeId(nodeId).success(function (data2) {
            if (data2 != null && data2 != "null") {
                $scope.entry = data2;
            }
        });

    }]);

angular.module("umbraco").factory("Our.Umbraco.IpFilter.Services.IpFilterService",
[
    '$http',
    'notificationsService',
    function ($http, notificationsService) {

        var service = {
            getEntryByNodeId: function (nodeId) {
                return $http.get("/umbraco/ipfilter/ipfilterapi/getentrybynodeid", {
                    params: { nodeId: nodeId }
                }).error(function () {
                    notificationsService.error("Oops", "There was an error retreiving the entry");
                });
            },
            saveEntry: function (entry) {
                return $http.post("/umbraco/ipfilter/ipfilterapi/saveentry", entry).error(function () {
                    notificationsService.error("Oops", "There was an error saving the entry");
                });
            }
        };

        return service;

    }]
);

angular.module("umbraco.directives")
     .directive("umbContentPicker", function (dialogService, entityResource) {
         return {
             restrict: "E",
             replace: true,
             templateUrl: "/App_Plugins/IpFilter/Views/ipFilter.contentPicker.html",
             require: "ngModel",
             link: function (scope, element, attr, ctrl) {

                 ctrl.$render = function () {
                     var val = parseInt(ctrl.$viewValue);
                     if (!isNaN(val) && angular.isNumber(val) && val > 0) {
                         entityResource.getById(val, "Document").then(function (item) {
                             scope.node = item;
                         });
                     }
                 };

                 scope.openPicker = function () {
                     dialogService.contentPicker({
                         callback: populateItem,
                         multipicker: false
                     });
                 }

                 scope.removeItem = function () {
                     scope.node = undefined;
                     updateModel(0);
                 }

                 function populateItem(item) {
                     scope.node = item;
                     updateModel(item.id);
                 }

                 function updateModel(id) {
                     ctrl.$setViewValue(id);
                 }

                 //scope.$watch(function () {
                 //    return ctrl.$modelValue;
                 //}, function (newValue) {
                 //    console.log(newValue);
                 //});
             }
         };
     });