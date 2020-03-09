function addCommentController($scope, $q, editorState, localizationService, userService, userGroupsResource, usersResource, formHelper, editorCommentsResource) {

    var vm = this;

    vm.loading = true;
    vm.submit = submit;
    vm.close = close;

    localizationService.localize('Our.Umbraco.EditorComments_AddComment').then(function (value) {
        vm.headerName = value;
    });

    userService.getCurrentUser().then(function (user) {
        vm.userId = user.id;
    });

    /*
     * Adds new comment.
     */
    function submit() {
        // only submit if form is valid
        if (formHelper.submitForm({ scope: $scope, formCtrl: this.addCommentForm })) {
            var msg = _.find(vm.comment.properties, function (p) { return p.alias === 'message'; }).value;
            var notify = _.find(vm.comment.properties, function (p) { return p.alias === 'notify'; }).value;
            var notifySelectedValue = (notify && notify.length > 0 ? notify[0] : null);            

            editorCommentsResource.createComment({
                ParentCommentId: vm.replyToCommentId,
                UmbracoNodeId: vm.nodeId,
                UmbracoNodePropertyId: vm.property.id,
                UserId: vm.userId,
                Message: msg,
                NotifyUser: (notifySelectedValue != null && notifySelectedValue.group === 'Users' ? notifySelectedValue.id : null),
                NotifyUserGroup: (notifySelectedValue != null && notifySelectedValue.group === 'User Groups' ? notifySelectedValue.id : null)
            }).then(function () {
                vm.close(true);
            });
        }
    }

    /*
     * Close the editor. 
     */
    function close(hasSubmitted) {
        if ($scope.model.close) {
            $scope.model.close(hasSubmitted);
        }
    }

    /*
     * Initialise the controller. 
     */
    function init() {
        // get propery and current node information
        vm.nodeId = editorState.current.id;
        vm.nodeVariant = _.find(editorState.current.variants, function (v) { return v.active; });
        vm.property = $scope.model.property;
        vm.replyToCommentId = $scope.model.replyToCommentId;

        // set title
        vm.contentTitle = vm.nodeVariant.name + ' - ' + vm.property.label;

        // get user groups and users available for selection in the notify dialog
        vm.groupsAndUsers = [];
        var userPromises = [];        

        userPromises.push(userGroupsResource.getUserGroups().then(function (groups) {
            groups.forEach(function (group) {
                vm.groupsAndUsers.push({ id: group.id, value: group.name, group: 'User Groups' });
            });
        }));

        userPromises.push(usersResource.getPagedResults({ pageSize: 9999 }).then(function (data) {
            data.items.forEach(function (user) {
                vm.groupsAndUsers.push({ id: user.id, value: user.name, group: 'Users' });
            });
        }));

        $q.all(userPromises).then(function () {
            // create empty comment form model
            vm.comment = {
                properties: [
                    {
                        alias: 'message',
                        label: 'Message',
                        validation: { mandatory: true },
                        view: 'textarea'
                    },
                    {
                        alias: 'notify',
                        label: 'Notify',
                        description: 'Sends notification of this message to individual user or user group.',
                        view: '/App_Plugins/Our.Umbraco.EditorComments/backoffice/views/notify-dropdown.html',
                        config: {
                            items: vm.groupsAndUsers
                        }
                    }
                ]
            };

            vm.loading = false;
        });
    }

    init();
}

angular.module('umbraco').controller('Our.Umbraco.EditorComments.AddCommentController', addCommentController);