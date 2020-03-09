function editorCommentsResource($http, umbRequestHelper) {

    return {
        getComments: function (umbracoNodeId, umbracoNodePropertyId) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl('editorCommentsBaseUrl', 'GetComments', { umbracoNodeId: umbracoNodeId, umbracoNodePropertyId: umbracoNodePropertyId })),
                'Failed to get comments');
        },

        getPendingCommentsCount: function (umbracoNodeId) {
            return umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl('editorCommentsBaseUrl', 'GetPendingCommentsCount', { umbracoNodeId: umbracoNodeId })),
                'Failed to get pending comments count');
        },

        createComment: function (comment) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl('editorCommentsBaseUrl', 'CreateComment'), comment),
                'Failed to create comment');
        },

        changeCommentStatus: function (commentId, status) {
            return umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl('editorCommentsBaseUrl', 'ChangeCommentStatus'), { commentId: commentId, status: status }),
                'Failed to change comment status');
        }
    };
}

angular.module('umbraco.resources').factory('editorCommentsResource', editorCommentsResource);
