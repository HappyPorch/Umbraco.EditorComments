angular.module('umbraco.services').config([
    '$httpProvider',
    function ($httpProvider) {

        $httpProvider.interceptors.push(function ($injector) {
            return {
                'response': function (response) {

                    if (response.config.url === '/umbraco/backoffice/UmbracoApi/Dashboard/GetDashboard?section=content' && response.data && response.data.length > 0) {
                        // if the Editor Comments dashboard exists, get the pending comments count and add it as a badge to the dashboard tab
                        var editorCommentsDashboard = _.find(response.data, function (d) { return d.alias === 'ourUmbracoEditorComments'; });

                        if (editorCommentsDashboard) {
                            var editorCommentsResource = $injector.get('editorCommentsResource');

                            editorCommentsResource.getPendingCommentsCount().then(function (count) {
                                var tabLink = $('[data-element="tab-ourUmbracoEditorComments"] a');

                                if (tabLink.length > 0) {
                                    var pendingBadge = tabLink.children('.umb-badge').first();

                                    if (pendingBadge.length === 0 && count > 0) {
                                        var linkHtml = tabLink.html();

                                        tabLink.html(linkHtml + '&nbsp;<div class="umb-badge umb-badge--xs umb-badge--warning">' + count + '</div>');
                                    }
                                    else if (pendingBadge.length > 0 && count === 0) {
                                        pendingBadge.remove();
                                    }
                                }
                            });
                        }
                    }

                    return response;
                }
            };
        });

    }
]);