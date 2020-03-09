using Our.Umbraco.EditorComments.Core.Dto;
using Our.Umbraco.EditorComments.Core.Extensions;
using Our.Umbraco.EditorComments.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.EditorComments.Core.Controllers.Api
{
    [PluginController("EditorComments")]
    public class EditorCommentsController : UmbracoAuthorizedApiController
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IContentSection _contentSection;
        private readonly IRuntimeState _runtimeState;

        private static readonly HtmlStringUtilities StringUtilities = new HtmlStringUtilities();

        public EditorCommentsController(IScopeProvider scopeProvider, IContentSection contentSection, IRuntimeState runtimeState)
        {
            _scopeProvider = scopeProvider;
            _contentSection = contentSection;
            _runtimeState = runtimeState;
        }

        [HttpPost]
        public bool CreateComment(CommentDto commentDto)
        {
            var comment = commentDto.ConvertToModel();

            if (comment == null)
            {
                throw new ArgumentNullException(nameof(commentDto));
            }

            var now = DateTime.Now;

            comment.CreateDate = now;
            comment.UpdateDate = now;

            if (comment.ParentCommentId.HasValue)
            {
                // status is not applicable for replies
                comment.Status = (int)CommentStatus.NotApplicable;
            }

            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Insert<Comment>(comment);

                scope.Complete();
            }

            SendNotification(commentDto);

            return true;
        }

        private void SendNotification(CommentDto commentDto)
        {
            if (!commentDto.ParentCommentId.HasValue && !commentDto.NotifyUser.HasValue && !commentDto.NotifyUserGroup.HasValue)
            {
                // no need to send a notification
                return;
            }

            var node = Services.ContentService.GetById(commentDto.UmbracoNodeId);
            var user = Services.UserService.GetUserById(commentDto.UserId);

            var notifyUsers = new List<IUser>();

            if (commentDto.NotifyUserGroup.HasValue)
            {
                notifyUsers.AddRange(Services.UserService.GetAllInGroup(commentDto.NotifyUserGroup.Value));
            }
            else if (commentDto.NotifyUser.HasValue)
            {
                notifyUsers.Add(Services.UserService.GetUserById(commentDto.NotifyUser.Value));
            }

            if (commentDto.ParentCommentId.HasValue)
            {
                // also notify the author of the comment that this is a reply to
                using (var scope = _scopeProvider.CreateScope())
                {
                    var parentComment = scope.Database.Query<Comment>()
                        .Where(c => c.Id == commentDto.ParentCommentId.Value)
                        .FirstOrDefault();

                    if (parentComment?.UserId > 0)
                    {
                        notifyUsers.Add(Services.UserService.GetUserById(parentComment.UserId));
                    }

                    scope.Complete();
                }
            }

            if (notifyUsers?.Any() != true)
            {
                return;
            }

            using (var smtp = new SmtpClient())
            {
                foreach (var notifyUser in notifyUsers)
                {
                    if (notifyUser == null || notifyUser.Id == user.Id)
                    {
                        // don't send a notification to the user who created the comment
                        continue;
                    }

                    var mailMessage = new MailMessage()
                    {
                        Subject = Services.TextService.Localize("Our.Umbraco.EditorComments.Email/Subject", notifyUser.GetUserCulture(Services.TextService, GlobalSettings)),
                        Body = Services.TextService.Localize(
                                    "Our.Umbraco.EditorComments.Email/Body",
                                    notifyUser.GetUserCulture(Services.TextService, GlobalSettings),
                                    new[] {
                                    notifyUser.Name,
                                    node.Name,
                                    user.Name,
                                    _runtimeState.ApplicationUrl.ToString(),
                                    node.Id.ToString(),
                                    StringUtilities.ReplaceLineBreaks(commentDto.Message).ToString(),
                                    commentDto.UmbracoNodePropertyName
                                    }),
                        IsBodyHtml = true,
                        From = new MailAddress(_contentSection.NotificationEmailAddress)
                    };

                    mailMessage.To.Add(notifyUser.Email);

                    try
                    {
                        smtp.Send(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error<EditorCommentsController>("Failed to send notification email to: " + notifyUser.Email, ex);
                    }
                }
            }
        }

        [HttpPost]
        public bool ChangeCommentStatus(ChangeCommentStatusDto changeStatusDto)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var comment = scope.Database.Query<Comment>()
                                    .First(c => c.Id == changeStatusDto.CommentId);

                if (comment == null)
                {
                    throw new EntityNotFoundException(nameof(changeStatusDto));
                }

                comment.Status = (int)changeStatusDto.Status;

                scope.Database.Update<Comment>(comment, c => c.Status);

                scope.Complete();
            }

            return true;
        }

        [HttpGet]
        public IList<CommentDto> GetComments(int? umbracoNodeId, int? umbracoNodePropertyId = null)
        {
            List<Comment> comments;

            using (var scope = _scopeProvider.CreateScope())
            {
                var query = scope.Database.Query<Comment>()
                              .Include(c => c.User)
                              .Include(c => c.UmbracoNode)
                              .Include(c => c.UmbracoNodeProperty.PropertyType)
                              .Where(c => true);

                if (umbracoNodeId.HasValue)
                {
                    query = query.Where(c => c.UmbracoNodeId == umbracoNodeId.Value);
                }

                if (umbracoNodePropertyId.HasValue)
                {
                    query = query.Where(c => c.UmbracoNodePropertyId == umbracoNodePropertyId.Value);
                }

                comments = query
                            .OrderBy(c => c.Status)
                            .ThenByDescending(c => c.CreateDate)
                            .ToList();

                scope.Complete();
            }

            if (comments?.Any() == false)
            {
                return new List<CommentDto>();
            }
            else
            {
                return comments.Where(c => c.ParentCommentId == null).Select(c => c.ConvertToDto(comments)).ToList();
            }
        }

        [HttpGet]
        public int GetPendingCommentsCount(int? umbracoNodeId)
        {
            var count = 0;

            using (var scope = _scopeProvider.CreateScope())
            {
                var query = scope.Database.Query<Comment>()
                              .Include(c => c.User)
                              .Include(c => c.UmbracoNode)
                              .Include(c => c.UmbracoNodeProperty.PropertyType)
                              .Where(c => c.Status == (int)CommentStatus.Pending);

                if (umbracoNodeId.HasValue)
                {
                    query = query.Where(c => c.UmbracoNodeId == umbracoNodeId.Value);
                }

                count = query.Count();

                scope.Complete();
            }

            return count;
        }
    }
}
