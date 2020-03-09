function contentAppController($rootScope, $scope, $compile, $element, $timeout, editorState, editorService, editorCommentsResource) {

    var vm = this;
    vm.markAsCompleted = markAsCompleted;
    vm.replyToComment = replyToComment;
    vm.loadComments = loadComments;
    vm.goToProperty = goToProperty;

    function initContentApp() {
        vm.showAllComments = (editorState.current === null);
        vm.nodeId = (!vm.showAllComments ? editorState.current.id : null);
        vm.disabled = (vm.nodeId === 0);

        $scope.$on('Our.Umbraco.EditorComments_UpdateContentApp', function (event) {
            vm.loadComments();
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
     * Opens form to add new reply.
     */
    function replyToComment(propertyId, propertyName, replyToCommentId) {
        editorService.open({
            property: { id: propertyId, label: propertyName },
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

    function initEditorCommentsButtons() {
        if (vm.disabled) {
            // don't create buttons when content app is disabled
            return;
        }

        var contentForm = angular.element($element.closest('form[name="contentForm"]'));

        if (contentForm.length === 0) {
            // not on a content node, so just load the comments
            loadComments();
            return;
        }

        var umbProperties = angular.element(contentForm.find('.umb-property'));

        if (umbProperties.length === 0) {
            // form not initialised yet, wait for a bit and try again
            $timeout(initEditorCommentsButtons, 100);
            return;
        }

        _.each(umbProperties, function (u) {
            var umbProperty = angular.element(u);

            // insert directive in the property label, before the last element (the description span)
            var controlLabel = umbProperty.find('label.control-label');

            controlLabel.children().last().before('<editor-comments-button></editor-comments-button>');

            $compile(controlLabel.contents())(controlLabel.scope());
        });

        loadComments();
    }

    function loadComments() {
        if (vm.disabled) {
            // don't load when content app is disabled
            return;
        }

        vm.loading = true;

        editorCommentsResource.getComments(vm.nodeId).then(function (comments) {
            vm.comments = comments;

            var groupedComments = _.groupBy(vm.comments, function (c) { return c.UmbracoNodePropertyId; });

            _.each(groupedComments, function (group) {
                // check if any comment is pending
                var hasPendingComments = _.some(group, function (c) { return !c.IsCompleted; });

                // update the button with the pending status
                var propertyId = group[0].UmbracoNodePropertyId;

                $rootScope.$broadcast('Our.Umbraco.EditorComments_UpdateButtonState_' + propertyId, hasPendingComments);
            });

            // show number of pending comments as a badge in the content app
            var pendingCommentCount = _.filter(vm.comments, function (c) { return !c.IsCompleted; }).length;

            if ($scope.model) {
                if (pendingCommentCount > 0) {
                    $scope.model.badge = {
                        count: pendingCommentCount,
                        type: 'warning'
                    };
                }
                else {
                    $scope.model.badge = null;
                }
            }

            vm.loading = false;
        });
    }

    function goToProperty($event, comment) {
        var newHash = '#/content/content/edit/' + comment.UmbracoNodeId + '?editor-comment-property=' + comment.UmbracoNodePropertyId;
        var hasHashChanged = (window.location.hash !== newHash);

        window.location.hash = newHash;

        if (!hasHashChanged) {
            // change back to content tab manually
            var contentColumn = angular.element($element.closest('#contentcolumn'));

            var contentContentAppTabButton = angular.element(contentColumn.find('button[data-element="sub-view-umbContent"]'));

            if (contentContentAppTabButton) {
                contentContentAppTabButton.trigger('click');
            }
        }

        $timeout(function () {
            // scroll down to selected property
            var subViewContent = angular.element('.sub-view-Content');

            if (subViewContent && typeof subViewContent.scope !== 'undefined') {
                subViewContent.scope().$broadcast('Our.Umbraco.EditorComments_ScrollToButton_' + comment.UmbracoNodePropertyId);
            }
        }, 500);

        $event.preventDefault();
        return false;
    }

    function init() {
        initContentApp();
        initEditorCommentsButtons();
    }

    init();

}

angular.module('umbraco').controller('Our.Umbraco.EditorComments.ContentAppController', contentAppController);
