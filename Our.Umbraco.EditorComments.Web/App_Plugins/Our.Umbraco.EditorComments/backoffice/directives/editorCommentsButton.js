function editorCommentsButton() {

    return {
        restrict: 'E',
        replace: true,
        template: '<button type="button" class="editor-comments__button" ng-class="{ \'editor-comments__button--warning\': showWarning }" localize="title,aria-label" aria-label="@Our.Umbraco.EditorComments_ViewOrAddEditorComments" title="@Our.Umbraco.EditorComments_ViewOrAddEditorComments" ng-click="openEditorComments(property)"><i class="icon icon-chat-active" aria-hidden="true"></i></button>',
        controller: ['$scope', '$element', 'editorService', function editorCommentsButtonController($scope, $element, editorService) {
            
            $scope.openEditorComments = function (property) {
                // open sidebar editor to view comments
                editorService.open({
                    property: property,
                    view: '/App_Plugins/Our.Umbraco.EditorComments/backoffice/views/view-comments.html',
                    size: 'small',
                    close: function () {
                        editorService.close();
                    }
                });
            };

            if ($scope.property) {
                // register event handler to update the button status
                $scope.$on('Our.Umbraco.EditorComments_UpdateButtonState_' + $scope.property.id, function (event, showWarning) {
                    $scope.showWarning = showWarning;
                });

                // register event handler to scroll to this button
                $scope.$on('Our.Umbraco.EditorComments_ScrollToButton_' + $scope.property.id, function (event) {
                    $element.get(0).scrollIntoView({ behavior: 'smooth', block: 'start' });
                });
            }

        }]
    };

}

angular.module('umbraco.directives').directive('editorCommentsButton', editorCommentsButton);
