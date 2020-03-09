function viewCommentsController($rootScope, $scope, $element, editorState, localizationService, userService, editorService, editorCommentsResource) {

    var vm = this;

    vm.loading = true;
    vm.addComment = addComment;
    vm.viewAllNodeComments = viewAllNodeComments;
    vm.close = close;
    vm.loadComments = loadComments;
    vm.markAsCompleted = markAsCompleted;

    localizationService.localize('Our.Umbraco.EditorComments_EditorComments').then(function (value) {
        vm.headerName = value;
    });

    userService.getCurrentUser().then(function (user) {
        vm.userId = user.id;
    });

    /*
     * Opens form to add new comment.
     */
    function addComment(replyToCommentId) {
        editorService.open({
            property: vm.property,
            replyToCommentId: replyToCommentId,
            view: '/App_Plugins/Our.Umbraco.EditorComments/backoffice/views/add-comment.html',
            size: 'small',
            close: function (hasSubmitted) {
                if (hasSubmitted) {
                    vm.loadComments();
                }

                editorService.close();                
            }
        });
    }

    /*
     * Find editor comments content app button and trigger a click to switch to it.
     */
    function viewAllNodeComments() {
        var contentColumn = angular.element($element.closest('#contentcolumn'));

        var editorCommentsContentAppTabButton = angular.element(contentColumn.find('button[data-element="sub-view-ourUmbracoEditorComments"]'));

        if (editorCommentsContentAppTabButton) {
            editorCommentsContentAppTabButton.trigger('click');
        }

        vm.close();
    }

    /*
     * Close the editor. 
     */
    function close() {
        if ($scope.model.close) {
            $scope.model.close();
        }
    }

    /*
     * Loads the current comments. 
     */
    function loadComments() {
        vm.loading = true;

        editorCommentsResource.getComments(vm.nodeId, vm.property.id).then(function (comments) {
            vm.comments = comments;
            
            // check if any comment is pending
            var hasPendingComments = _.some(vm.comments, function (c) { return !c.IsCompleted; });

            // update the button with the pending status
            $rootScope.$broadcast('Our.Umbraco.EditorComments_UpdateButtonState_' + vm.property.id, hasPendingComments);

            // update the content app
            $rootScope.$broadcast('Our.Umbraco.EditorComments_UpdateContentApp');

            vm.loading = false;
        });
    }

    const completedCommentStatus = 10;

    /*
     * Marks a comment as completed. 
     */
    function markAsCompleted(commentId) {
        editorCommentsResource.changeCommentStatus(commentId, completedCommentStatus).then(function () {
            vm.loadComments();
        });
    }

    /*
     * Initialise the controller. 
     */
    function init() {
        // get propery and current node information
        vm.nodeId = editorState.current.id;
        vm.nodeVariant = _.find(editorState.current.variants, function (v) { return v.active; });
        vm.property = $scope.model.property;

        // set title
        vm.contentTitle = vm.nodeVariant.name + ' - ' + vm.property.label;

        vm.loadComments();
    }

    init();
}

angular.module('umbraco').controller('Our.Umbraco.EditorComments.ViewCommentsController', viewCommentsController);