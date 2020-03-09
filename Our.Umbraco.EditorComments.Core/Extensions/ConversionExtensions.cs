using Our.Umbraco.EditorComments.Core.Dto;
using Our.Umbraco.EditorComments.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web;

namespace Our.Umbraco.EditorComments.Core.Extensions
{
    internal static class ConversionExtensions
    {
        private static readonly HtmlStringUtilities StringUtilities = new HtmlStringUtilities();

        /// <summary>
        /// Converts a Comment object to a CommentDto object.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        public static CommentDto ConvertToDto(this Comment comment, IList<Comment> comments)
        {
            if (comment == null)
            {
                return null;
            }

            return new CommentDto()
            {
                Id = comment.Id,
                ParentCommentId = comment.ParentCommentId,
                UmbracoNodeId = comment.UmbracoNodeId,
                UmbracoNodeName = comment.UmbracoNode.Text,
                UmbracoNodePropertyId = comment.UmbracoNodePropertyId,
                UmbracoNodePropertyName = comment.UmbracoNodeProperty.PropertyType.Name,
                UserId = comment.User.Id,
                UserName = comment.User.UserName,
                Message = comment.Message,
                Status = (CommentStatus)comment.Status,
                CreateDate = comment.CreateDate,
                UpdateDate = comment.UpdateDate,
                Replies = comments?.Where(c => c.ParentCommentId == comment.Id).OrderBy(c => c.CreateDate).Select(c => c.ConvertToDto(null)).ToList()
            };
        }

        /// <summary>
        /// Converts a CommentDto object to a Comment object.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static Comment ConvertToModel(this CommentDto commentDto)
        {
            if (commentDto == null)
            {
                return null;
            }

            return new Comment()
            {
                Id = commentDto.Id,
                ParentCommentId = commentDto.ParentCommentId,
                UmbracoNodeId = commentDto.UmbracoNodeId,
                UmbracoNodePropertyId = commentDto.UmbracoNodePropertyId,
                UserId = commentDto.UserId,
                Message = StringUtilities.ReplaceLineBreaks(commentDto.Message).ToString(),
                Status = (int)commentDto.Status,
                CreateDate = commentDto.CreateDate,
                UpdateDate = commentDto.UpdateDate
            };
        }
    }
}
